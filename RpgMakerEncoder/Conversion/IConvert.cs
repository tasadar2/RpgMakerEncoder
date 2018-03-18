namespace RpgMakerEncoder.Conversion
{
    public interface IConvert<in TSource, out TDestination>
    {
        TDestination Convert(TSource source);
    }
}