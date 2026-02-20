using System;

namespace APTPackageDependenciesResolver.Test;

[TestClass]
public class FormatThrowHelperTest
{
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
