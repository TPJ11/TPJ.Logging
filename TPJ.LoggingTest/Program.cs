
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TPJ.Logging;
using TPJ.LoggingTest.Models;

namespace TPJ.LoggingTest;

class Program
{
    private static ILogger _logger = default!;

    static async Task Main(string[] args)
    {
        SetUp();

        LogWithCustomNote();
        LogSensitive();
        LogScope();
        LogDivideByZero();
        LogArrayOfArrays();
        LogNestedTypes();
    }
    private static void SetUp()
    {
        var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var configuration = builder.Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddTPJLogging();

        var serviceProvider = services.BuildServiceProvider();

        _logger = serviceProvider.GetRequiredService<ILogger>();
    }

    private static void LogWithCustomNote()
    {
        _logger.Log(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Custom Note"), "super epic note");
    }

    private static void LogSensitive()
    {
        _logger.Log(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Sensitive"), new TestSensitive()
        {
            UserName = "Test User Name",
            Password = "Some Strong Password"
        });
    }

    private static void LogScope()
    {
        _logger.Log(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Scope"), new Scope());
    }

    private static void LogDivideByZero()
    {
        var divide = new Divide(1, 0);
        try
        {

            var result =  divide.ValueOne / divide.ValueTwo;
        }
        catch (Exception e)
        {
            _logger.Log(System.Reflection.MethodBase.GetCurrentMethod(), e, divide);
        }
    }

    private static void LogArrayOfArrays()
    {
        _logger.Log(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Test"), new[] { new ListType()
        {
            SomeArray = new InnerListItem[2]
            {
                new InnerListItem()
                {
                    Name = "Array Item 1",
                    InnerList = new List<InnerListItem>()
                    {
                        new InnerListItem()
                        {
                            Name = "Array Item 1 Inner Item 1"
                        }
                    }
                },
                new InnerListItem()
                {
                    Name = "Array Item 2"
                }
            },
            SomeICollection = new List<InnerListItem>()
            {
                new InnerListItem()
                {
                    Name = "ICollection Item 1",
                    InnerList = new List<InnerListItem>()
                    {
                        new InnerListItem()
                        {
                            Name = "ICollection Item 1 Inner Item 1"
                        }
                    }
                },
                new InnerListItem()
                {
                    Name = "ICollection Item 2"
                }
            },
            SomeIList = new List<InnerListItem>()
            {
                new InnerListItem()
                {
                    Name = "IList Item 1",
                    InnerList = new List<InnerListItem>()
                    {
                        new InnerListItem()
                        {
                            Name = "IList Item 1 Inner Item 1"
                        }
                    }
                },
                new InnerListItem()
                {
                    Name = "IList Item 2"
                }
            },
            SomeList = new List<InnerListItem>()
            {
                new InnerListItem()
                {
                    Name = "List Item 1",
                    InnerList = new List<InnerListItem>()
                    {
                        new InnerListItem()
                        {
                            Name = "List Item 1 Inner Item 1"
                        }
                    }
                },
                new InnerListItem()
                {
                    Name = "List Item 2"
                }
            },
            SomeListEnumerable = new List<InnerListItem>()
            {
                new InnerListItem()
                {
                    Name = "Enumerable Item 1",
                    InnerList = new List<InnerListItem>()
                    {
                        new InnerListItem()
                        {
                            Name = "Enumerable Item 1 Inner Item 1"
                        }
                    }
                },
                new InnerListItem()
                {
                    Name = "Enumerable Item 2"
                }
            },
        }});
    }

    private static void LogNestedTypes()
    {
        var nested = new Types()
        {
            NestedClass = new Types()
            {
                NestedClass = new Types()
                {
                    NestedClass = new Types()
                    {
                        NestedClass = new Types(),
                        ListOfTypes = new List<Types>() { new Types(), new Types() }
                    }
                }
            }
        };

        _logger.Log(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Test"), nested);
    }

}