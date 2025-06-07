using DevToys.JsonToPython.Converters;

namespace DevToys.JsonToPythonTests.Converters
{
    [TestClass()]
    public class JsonToPythonConverterTests
    {
        [TestMethod()]
        public void ConvertTest()
        {

            var json = """
                {
                  "name": "John Doe",
                  "age": 30,
                  "city": "New York",
                  "address": {
                    "street": "123 Main St",
                    "zipcode": "10001"
                  },
                  "hobbies": ["reading", "hiking", "cooking"],
                  "span1": {
                    "start": {"y":1990, "m":8, "d":2},
                    "end": {"y":2000, "d":1, "m":18}
                  },
                  "span2": {
                    "start": {"d":2010, "m":6, "y":5},
                    "end": {"m":2020, "y":4, "d":26}
                  },
                  "customers": [
                      {
                        "street": "123 Main St",
                        "zipcode": "10001"
                      },
                      {
                        "zipcode": "10001",
                        "street": "123 Main St"
                        },
                      {
                        "street": "123 Main St",
                        "zipcode": "10001"
                      }
                  ]
                }
                """;

            var python = """
                from typing import TypedDict


                class Address(TypedDict):
                    street: str
                    zipcode: str

                class Start(TypedDict):
                    y: int | float
                    m: int | float
                    d: int | float

                class Span1(TypedDict):
                    start: Start
                    end: Start

                class JsonRootElement(TypedDict):
                    name: str
                    age: int | float
                    city: str
                    address: Address
                    hobbies: list[str]
                    span1: Span1
                    span2: Span1
                    customers: list[Address]
                
                """;

            var converter = new JsonToPythonConverter(json, PythonDataType.TypedDict);
            var convertedPython = converter.Convert();

            Console.WriteLine(python);
            Assert.AreEqual(python, convertedPython);
        }
    }
}