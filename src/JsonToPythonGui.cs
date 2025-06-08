using DevToys.Api;
using DevToys.JsonToPython.Converters;
using System.ComponentModel.Composition;
using System.Diagnostics;
using static DevToys.Api.GUI;

namespace DevToys.JsonToPython;

[Export(typeof(IGuiTool))]
[Name("JsonToPythonExtension")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF0E1',// '\uEE79', '\uF0CC' F0E1 E0DD
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(JsonToPythonAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.JsonToPython.JsonToPythonExtension",
    ShortDisplayTitleResourceName = nameof(JsonToPythonExtension.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(JsonToPythonExtension.LongDisplayTitle),
    DescriptionResourceName = nameof(JsonToPythonExtension.Description),
    AccessibleNameResourceName = nameof(JsonToPythonExtension.AccessibleName))]
internal sealed class JsonToPythonGui : IGuiTool
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIMultiLineTextInput _inputTextArea = MultiLineTextInput("json-to-python-input-text-area");
    private readonly IUIMultiLineTextInput _outputTextArea = MultiLineTextInput("json-to-python-output-text-area");
    private static readonly SettingDefinition<PythonDataType> _pythonDataTypeDefinition = new(name: "Output type", defaultValue: PythonDataType.TypedDict);
    private static readonly SettingDefinition<NumberType> _numberTypeDefinition = new(name: "Number type", defaultValue: NumberType.Union);

    [ImportingConstructor]
    public JsonToPythonGui(ISettingsProvider settingsProvider)
    {
        this._settingsProvider = settingsProvider;
    }

    private enum GridColumn
    {
        Content
    }

    private enum GridRow
    {
        Header,
        Content,
        Footer
    }

    public UIToolView View => new
        (
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Header, Auto),
                    (GridRow.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Columns(
                    (GridColumn.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
            .Cells(
                Cell(
                    GridRow.Header,
                    GridColumn.Content,
                    Stack()
                        .Vertical()
                        .LargeSpacing()
                        .WithChildren(
                            Label().Text(JsonToPythonExtension.ConvertJsonToPythonConfigurationTitle),
                            Setting()
                                .Icon("FluentSystemIcons", '\uECF4')
                                .Title("Output type")
                                .Description("Select Python data type")
                                .Handle(
                                    this._settingsProvider,
                                    _pythonDataTypeDefinition,
                                    this.OnPythonDataTypeChanged,
                                    Item("TypedDict", PythonDataType.TypedDict),
                                    Item("dataclass", PythonDataType.DataClass),
                                    Item("pydantic", PythonDataType.Pydantic)
                                ),
                            Setting()
                                .Icon("FluentSystemIcons", '\uE748')
                                .Title("Number type")
                                .Description("Select Number data type")
                                .Handle(
                                    this._settingsProvider,
                                    _numberTypeDefinition,
                                    this.OnNumberTypeChanged,
                                    Item("Union", NumberType.Union),
                                    Item("Define Number type", NumberType.DefineNumber),
                                    Item("As int", NumberType.Int),
                                    Item("As float", NumberType.Float)
                                )
                        )
                ),
                Cell(
                    GridRow.Content,
                    GridColumn.Content,
                    SplitGrid()
                        .Vertical()
                        .WithLeftPaneChild(
                            this._inputTextArea
                                .Title(JsonToPythonExtension.ConvertJsonToPythonInputTitle)
                                .Language("json")
                                .OnTextChanged(this.OnInputTextChanged))
                        .WithRightPaneChild(
                            this._outputTextArea
                                .Title(JsonToPythonExtension.ConvertJsonToPythonOutputTitle)
                                .Language("python")
                                .ReadOnly()
                                .Extendable()
                        )
                )
            )
        );

    public void OnDataReceived(string dataTypeName, object? parsedData) => throw new NotImplementedException();

    private void OnPythonDataTypeChanged(PythonDataType dataType)
    {
        this.Convert();
    }

    private void OnNumberTypeChanged(NumberType numberType)
    {
        this.Convert();
    }

    private void OnInputTextChanged(string text)
    {
        this.Convert();
    }

    private void Convert()
    {
        var json = this._inputTextArea.Text;
        var outputType = _settingsProvider.GetSetting(_pythonDataTypeDefinition);
        var numberType = _settingsProvider.GetSetting(_numberTypeDefinition);

        if (string.IsNullOrEmpty(json))
        {
            this._outputTextArea.Text(string.Empty);
            return;
        }

        try
        {
            var converter = new JsonToPythonConverter(json, outputType, numberType);
            this._outputTextArea.Text(converter.Convert());
        }
        catch
        {
            this._outputTextArea.Text("Please provide a valid JSON");
        }
    }
}