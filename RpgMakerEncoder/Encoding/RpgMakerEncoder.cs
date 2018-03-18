using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RpgMakerEncoder.Conversion;
using RpgMakerEncoder.IO;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.Encoding
{
    public class RpgMakerEncoder
    {
        private readonly IJsonFileProcessor _jsonFileProcessor;
        private readonly IRubyEncoder _rubyEncoder;
        private readonly IConvert<RubyToken, JToken> _sourceConverter;
        private readonly IConvert<JToken, RubyToken> _gameConverter;
        private int _totalOperations = -1;
        private int _completedOperations;

        public RubyEncoderOptions EncoderOptions { get; set; }
        public RubyDecoderOptions DecoderOptions { get; set; }
        public string GamePath { get; set; }
        public string SourcePath { get; set; }

        public event EventHandler<OperationsCompleteEventArgs> OperationsComplete;
        public event EventHandler<OperationsProgressEventArgs> OperationsProgress;

        public RpgMakerEncoder(string gamePath, string sourcePath)
            : this(gamePath, sourcePath, new RubyEncoder(), new JTokenConverter(), new RubyTokenConverter(), new RpgMakerJsonFileProcessor())
        { }

        public RpgMakerEncoder(string gamePath, string sourcePath, IRubyEncoder rubyEncoder, IConvert<RubyToken, JToken> sourceConverter, IConvert<JToken, RubyToken> gameConverter, IJsonFileProcessor jsonFileProcessor)
        {
            _jsonFileProcessor = jsonFileProcessor;
            _rubyEncoder = rubyEncoder;
            _sourceConverter = sourceConverter;
            _gameConverter = gameConverter;
            GamePath = gamePath;
            SourcePath = sourcePath;
        }

        public void Encode()
        {
            var total = 0;
            foreach (var file in Directory.EnumerateFiles(Path.Combine(SourcePath, "Data"), "*.json", SearchOption.AllDirectories))
            {
                Task.Run(() => Encode(file)).ContinueWith(t => OperationComplete());
                total++;
            }
            StartOperationProgress(total);
        }

        public void Encode(string sourceFile)
        {
            var sourceToken = _jsonFileProcessor.Load(sourceFile);

            var gameToken = _gameConverter.Convert(sourceToken);

            var relativeFile = DirectoryHelper.MakeRelative(SourcePath, sourceFile);
            var gameFile = Path.GetFullPath(Path.Combine(GamePath, relativeFile));
            gameFile = Path.ChangeExtension(gameFile, "rxdata");

            _rubyEncoder.Encode(gameToken, gameFile, EncoderOptions);
        }

        public void Decode()
        {
            var total = 0;
            foreach (var file in Directory.EnumerateFiles(Path.Combine(GamePath, "Data"), "*.rxdata", SearchOption.AllDirectories))
            {
                Task.Run(() => Decode(file)).ContinueWith(t => OperationComplete());
                total++;
            }
            StartOperationProgress(total);
        }

        public void Decode(string gameFile)
        {
            var gameToken = _rubyEncoder.Decode(gameFile, DecoderOptions);

            var sourceToken = _sourceConverter.Convert(gameToken);

            var relativeFile = DirectoryHelper.MakeRelative(GamePath, gameFile);
            var sourceFile = Path.GetFullPath(Path.Combine(SourcePath, relativeFile));
            sourceFile = Path.ChangeExtension(sourceFile, "json");

            _jsonFileProcessor.Save(sourceToken, sourceFile);
        }

        protected virtual void OnOperationsComplete()
        {
            OperationsComplete?.Invoke(this, new OperationsCompleteEventArgs());
        }

        protected virtual void OnOperationsProgress(int count, int total)
        {
            OperationsProgress?.Invoke(this,
                                       new OperationsProgressEventArgs
                                       {
                                           Count = count,
                                           Total = total
                                       });
        }

        private void StartOperationProgress(int total)
        {
            _completedOperations = 0;
            _totalOperations = total;
            OperationComplete();
        }

        private void OperationComplete()
        {
            var completed = Interlocked.Increment(ref _completedOperations);
            if (_totalOperations > -1 && completed == _totalOperations)
            {
                _totalOperations = -1;
                OnOperationsComplete();
            }
            else
            {
                OnOperationsProgress(completed, _totalOperations);
            }
        }
    }
}