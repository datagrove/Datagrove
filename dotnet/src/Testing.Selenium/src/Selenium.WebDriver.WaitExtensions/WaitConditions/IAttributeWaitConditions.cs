namespace Datagrove.Testing.Selenium
{
    public interface IAttributeWaitConditions
    {
        bool ToContain(string attrName);
        bool ToContainWithValue(string attrName, string attrValue);
        bool ToNotContain(string attrName);
        bool ToContainWithoutValue(string attrName, string attrValue);
    }
}