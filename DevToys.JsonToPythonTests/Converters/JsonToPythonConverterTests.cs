using DevToys.JsonToPython.Converters;

namespace DevToys.JsonToPythonTests.Converters
{
    [TestClass()]
    public class JsonToPythonConverterTests
    {
        internal void ConvertTestCore(PythonDataType dataType, NumberType numberType, string expected)
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
                  "span": {
                    "start": {"y":1990, "m":8, "d":2},
                    "end": {"m":2, "d":15, "y":2020}
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
                  ],
                  "metadata": {
                    "span": {
                        "offset": 10,
                        "count": 50
                    }
                  }
                }
                """;
            var converter = new JsonToPythonConverter(json, dataType, numberType);
            var convertedPython = converter.Convert();

            Assert.AreEqual(expected, convertedPython);
        }

        [TestMethod()]
        public void ConvertDictUnionTest() => this.ConvertTestCore(PythonDataType.TypedDict, NumberType.Union, """
            from typing import TypedDict


            class Address(TypedDict):
                street: str
                zipcode: str

            class Start(TypedDict):
                y: int | float
                m: int | float
                d: int | float

            class Span(TypedDict):
                start: Start
                end: Start

            class Span2(TypedDict):
                offset: int | float
                count: int | float

            class Metadata(TypedDict):
                span: Span2

            class JsonRootElement(TypedDict):
                name: str
                age: int | float
                city: str
                address: Address
                hobbies: list[str]
                span: Span
                customers: list[Address]
                metadata: Metadata
            
            """);

        [TestMethod()]
        public void ConvertDictFuncUnionTest() => this.ConvertTestCore(PythonDataType.TypedDictFunction, NumberType.Union, """
            from typing import TypedDict


            Address = TypedDict('Address', {
                'street': str,
                'zipcode': str
                })

            Start = TypedDict('Start', {
                'y': int | float,
                'm': int | float,
                'd': int | float
                })

            Span = TypedDict('Span', {
                'start': Start,
                'end': Start
                })

            Span2 = TypedDict('Span2', {
                'offset': int | float,
                'count': int | float
                })

            Metadata = TypedDict('Metadata', {
                'span': Span2
                })

            JsonRootElement = TypedDict('JsonRootElement', {
                'name': str,
                'age': int | float,
                'city': str,
                'address': Address,
                'hobbies': list[str],
                'span': Span,
                'customers': list[Address],
                'metadata': Metadata
                })

            """);

        [TestMethod()]
        public void ConvertDataClassUnionTest() => this.ConvertTestCore(PythonDataType.DataClass, NumberType.Union, """
            from dataclasses import dataclass


            @dataclass
            class Address:
                street: str
                zipcode: str

            @dataclass
            class Start:
                y: int | float
                m: int | float
                d: int | float

            @dataclass
            class Span:
                start: Start
                end: Start

            @dataclass
            class Span2:
                offset: int | float
                count: int | float

            @dataclass
            class Metadata:
                span: Span2

            @dataclass
            class JsonRootElement:
                name: str
                age: int | float
                city: str
                address: Address
                hobbies: list[str]
                span: Span
                customers: list[Address]
                metadata: Metadata
            
            """);

        [TestMethod()]
        public void ConvertPydanticUnionTest() => this.ConvertTestCore(PythonDataType.Pydantic, NumberType.Union, """
            from pydantic import BaseModel


            class Address(BaseModel):
                street: str
                zipcode: str

            class Start(BaseModel):
                y: int | float
                m: int | float
                d: int | float

            class Span(BaseModel):
                start: Start
                end: Start

            class Span2(BaseModel):
                offset: int | float
                count: int | float

            class Metadata(BaseModel):
                span: Span2

            class JsonRootElement(BaseModel):
                name: str
                age: int | float
                city: str
                address: Address
                hobbies: list[str]
                span: Span
                customers: list[Address]
                metadata: Metadata
            
            """);

        [TestMethod()]
        public void ConvertDictDefineTest() => this.ConvertTestCore(PythonDataType.TypedDict, NumberType.DefineNumber, """
            from typing import TypedDict


            type Number = int | float

            class Address(TypedDict):
                street: str
                zipcode: str

            class Start(TypedDict):
                y: Number
                m: Number
                d: Number

            class Span(TypedDict):
                start: Start
                end: Start

            class Span2(TypedDict):
                offset: Number
                count: Number

            class Metadata(TypedDict):
                span: Span2

            class JsonRootElement(TypedDict):
                name: str
                age: Number
                city: str
                address: Address
                hobbies: list[str]
                span: Span
                customers: list[Address]
                metadata: Metadata
            
            """);

        [TestMethod()]
        public void ConvertDictIntTest() => this.ConvertTestCore(PythonDataType.TypedDict, NumberType.Int, """
            from typing import TypedDict


            class Address(TypedDict):
                street: str
                zipcode: str

            class Start(TypedDict):
                y: int
                m: int
                d: int

            class Span(TypedDict):
                start: Start
                end: Start

            class Span2(TypedDict):
                offset: int
                count: int

            class Metadata(TypedDict):
                span: Span2

            class JsonRootElement(TypedDict):
                name: str
                age: int
                city: str
                address: Address
                hobbies: list[str]
                span: Span
                customers: list[Address]
                metadata: Metadata
            
            """);

        [TestMethod()]
        public void ConvertDictFloatTest() => this.ConvertTestCore(PythonDataType.TypedDict, NumberType.Float, """
            from typing import TypedDict


            class Address(TypedDict):
                street: str
                zipcode: str

            class Start(TypedDict):
                y: float
                m: float
                d: float

            class Span(TypedDict):
                start: Start
                end: Start

            class Span2(TypedDict):
                offset: float
                count: float

            class Metadata(TypedDict):
                span: Span2

            class JsonRootElement(TypedDict):
                name: str
                age: float
                city: str
                address: Address
                hobbies: list[str]
                span: Span
                customers: list[Address]
                metadata: Metadata
            
            """);
    }
}