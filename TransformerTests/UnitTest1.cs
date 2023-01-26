using StudentDataTransformer;

namespace TransformerTests;

public class TransformerServiceTest
{

    [Test]
    public void ShouldGreet()
    {
        var greeting = TransformerService.Greet();
        Assert.That(greeting, Is.EqualTo("Hello"));
    }
}