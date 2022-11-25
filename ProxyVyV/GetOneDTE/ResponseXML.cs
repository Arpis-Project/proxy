
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
using System.Xml;
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class GetOneResponse
{

    private string mensajeField;

    private object docField;

    private object pdfField;

    /// <remarks/>
    public string mensaje
    {
        get
        {
            return this.mensajeField;
        }
        set
        {
            this.mensajeField = value;
        }
    }

    /// <remarks/>
    public object doc
    {
        get
        {
            return this.docField;
        }
        set
        {
            this.docField = value;
        }
    }

    /// <remarks/>
    public object pdf
    {
        get
        {
            return this.pdfField;
        }
        set
        {
            this.pdfField = value;
        }
    }
}

