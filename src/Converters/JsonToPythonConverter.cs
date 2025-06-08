using DevToys.Api;
using System.Text;
using System.Text.Json;

namespace DevToys.JsonToPython.Converters;

public enum PythonDataType
{
    DataClass,
    TypedDict,
    Pydantic
}

public enum NumberType
{
    Union,
    DefineNumber,
    Int,
    Float
}

public class JsonToPythonConverter(string json, PythonDataType outputType = PythonDataType.TypedDict, NumberType numberType = NumberType.Union)
{
    private readonly string _json = json;
    private readonly PythonDataType _outputType = outputType;
    private readonly NumberType _numberType = numberType;
    private readonly string _indent = "    ";
    private readonly string _rootNameBase = "JsonRootElement";
    private readonly string _numberTypeName = "Number";

    private bool _useAny = false;
    private bool _defineNumber = false;
    internal void InitFlag()
    {
        this._useAny = false;
        this._defineNumber = false;
    }

    public string Convert()
    {
        try
        {
            this.InitFlag();

            using var document = JsonDocument.Parse(this._json, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });

            var classCodes = new List<(string ClassName, string Code, int Hash)>();

            // check root class name
            var rootName = this._rootNameBase;

            var index = 1;
            while (this._json.Contains(rootName))
            {
                index++;
                rootName = $"{this._rootNameBase}{index}";
            }

            // convert json to python
            this.CreateClassDefinition(document.RootElement, rootName, classCodes);

            // check importing packages
            var importBuilder = new StringBuilder();

            var typingClasses = new List<string>();
            var importLines = new List<string>();

            if (this._useAny)
            {
                typingClasses.Add("Any");
            }

            switch (this._outputType)
            {
                case PythonDataType.TypedDict:
                    typingClasses.Add("TypedDict");
                    break;
                case PythonDataType.DataClass:
                    importLines.Add("from dataclasses import dataclass");
                    break;
                case PythonDataType.Pydantic:
                    importLines.Add("from pydantic import BaseModel");
                    break;
            }

            if (typingClasses.Count > 0)
            {
                importLines.Add($"from typing import {string.Join(", ", typingClasses)}");
            }

            if (importLines.Count > 0)
            {
                // for 2 empty line
                importLines.AddRange(["", ""]);
            }

            if (this._defineNumber)
            {
                importLines.AddRange(["type Number = int | float", ""]);
            }

            importLines.AddRange(classCodes.Select(item => item.Code));

            return string.Join(Environment.NewLine, importLines);
        }catch(Exception ex)
        {
            return ex.Message;
        }
    }

    internal string ToPythonType(JsonValueKind kind)
    {
        var typeName = kind switch
        {
            JsonValueKind.String => "str",
            JsonValueKind.Array => "list",
            JsonValueKind.Number => this._numberType switch
            {
                NumberType.Union => "int | float",
                NumberType.Int => "int",
                NumberType.Float => "float",
                NumberType.DefineNumber => this._numberTypeName,
                _ => throw new NotImplementedException()
            },
            JsonValueKind.True => "bool",
            JsonValueKind.False => "bool",
            _ => "Any"
        };

        this._useAny |= typeName == "Any";
        this._defineNumber |= typeName == this._numberTypeName && this._numberType == NumberType.DefineNumber;

        return typeName;
    }

    internal int SimpleConbineHash(params int[] hashes)
    {
        // To ignore property order, simply sum hash codes
        var baseHash = 0;
        foreach (var hash in hashes)
        {
            baseHash = unchecked(baseHash ^ hash);
        }

        return baseHash;
    }

    internal (string Name, int Hash) CreateClassDefinition(JsonElement element, string elementName, List<(string ClassName, string Code, int Hash)> outputList)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException();
        }

        var headerBuilder = new StringBuilder();
        var memberBuilder = new StringBuilder();

        // for check already defined
        var hash = 0;

        foreach (var property in element.EnumerateObject())
        {
            int propertyTypeHash;
            string propertyTypeName;

            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Object:
                    (propertyTypeName, propertyTypeHash) = this.CreateClassDefinition(property.Value, property.Name.ToPascalCase(), outputList);
                    break;

                case JsonValueKind.Array:
                    if (property.Value.GetArrayLength() == 0)
                    {
                        propertyTypeHash = JsonValueKind.Array.GetHashCode();
                        propertyTypeName = "list";
                    }
                    else
                    {
                        var listItemElement = property.Value.EnumerateArray().First();
                        string itemType;
                        int itemHash;

                        if (listItemElement.ValueKind == JsonValueKind.Object)
                        {
                            (itemType, itemHash) = this.CreateClassDefinition(listItemElement, property.Name.ToPascalCase() + "Item", outputList);
                        }
                        else
                        {
                            itemType = this.ToPythonType(listItemElement.ValueKind);
                            itemHash = listItemElement.ValueKind.GetHashCode();

                        }

                        propertyTypeName = $"list[{itemType}]";
                        propertyTypeHash = HashCode.Combine(JsonValueKind.Array, itemHash);
                    }

                    break;
                default:
                    propertyTypeHash = property.Value.ValueKind.GetHashCode();
                    propertyTypeName = this.ToPythonType(property.Value.ValueKind);
                    break;

            }

            hash = this.SimpleConbineHash(hash, HashCode.Combine(property.Name, propertyTypeHash));
            memberBuilder.AppendLine($"{this._indent}{property.Name}: {propertyTypeName}");

        }

        // search same definition
        var definedItem = outputList.FirstOrDefault(item => item.Hash == hash);

        if (definedItem == default)
        {
            // not found definition
            var classNameBase = elementName.ToPascalCase();
            var className = classNameBase;

            // check class name
            var index = 1;
            while (outputList.Any(item => item.ClassName == className) || (this._numberType == NumberType.DefineNumber && className == this._numberTypeName))
            {
                index++;
                className = $"{classNameBase}{index}";
            }

            switch (this._outputType)
            {
                case PythonDataType.TypedDict:
                    headerBuilder.AppendLine($"class {className}(TypedDict):");
                    break;
                case PythonDataType.DataClass:
                    headerBuilder.AppendLine("@dataclass");
                    headerBuilder.AppendLine($"class {className}:");
                    break;
                case PythonDataType.Pydantic:
                    headerBuilder.AppendLine($"class {className}(BaseModel):");
                    break;
            }

            headerBuilder.Append(memberBuilder);

            outputList.Add((className, headerBuilder.ToString(), hash));

            return (className, hash);
        }

        // retrun found item;
        return (definedItem.ClassName, definedItem.Hash);

    }
}

