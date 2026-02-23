using System;

namespace APTPackageDependenciesResolver;

[TestClass]
public class FormatThrowHelperTest
{
    // I'm currently lazy to write a comprehensive test for all cases of the formatted throw helper.
    // So this is a brief test just to make sure the formatting works as expected. I will write more tests in the future when I have more time.
    [TestMethod]
    public void SampleFormat()
    {
        Exception? thrownException = null;
        const string SampleFormat =
        """
        This is where the mistake happen!
        """;
        try
        {
            ThrowHelper.ThrowInvalidFormat(SampleFormat.AsSpan(), SampleFormat.AsSpan()[18..26], "Mistake isn't allowed");
        }
        catch (Exception e)
        {
            thrownException = e;
        }

        Assert.IsNotNull(thrownException);
        Assert.IsInstanceOfType(thrownException, typeof(FormatException));
        Assert.AreEqual(
        """
        Mistake isn't allowed

        This is where the mistake happen!
                          ^^^^^^^^
        """, thrownException.Message);
    }
}
