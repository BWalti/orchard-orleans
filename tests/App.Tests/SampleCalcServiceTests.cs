using Grains;
using Xunit;

public class SampleCalcServiceTests
{
    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(5, 3, 8)]
    [InlineData(2, 2, 4)]
    public void Adding(int a, int b, int expected)
    {
        var testee = new SampleCalcService();

        var result = testee.Add(a, b);

        Assert.Equal(expected, result);
    }
}
