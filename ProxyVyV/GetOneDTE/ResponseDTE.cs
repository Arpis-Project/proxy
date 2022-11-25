
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.sii.cl/SiiDte", IsNullable = false)]
public partial class GetOneResponseDTE
{

    private DTEDocumento documentoField;

    private decimal versionField;

    /// <remarks/>
    public DTEDocumento Documento
    {
        get
        {
            return this.documentoField;
        }
        set
        {
            this.documentoField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
public partial class DTEDocumento
{

    private DTEDocumentoEncabezado encabezadoField;

    private string idField;

    /// <remarks/>
    public DTEDocumentoEncabezado Encabezado
    {
        get
        {
            return this.encabezadoField;
        }
        set
        {
            this.encabezadoField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ID
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
public partial class DTEDocumentoEncabezado
{

    private DTEDocumentoEncabezadoIdDoc idDocField;

    /// <remarks/>
    public DTEDocumentoEncabezadoIdDoc IdDoc
    {
        get
        {
            return this.idDocField;
        }
        set
        {
            this.idDocField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
public partial class DTEDocumentoEncabezadoIdDoc
{

    private string tipoDTEField;

    private string folioField;

    /// <remarks/>
    public string TipoDTE
    {
        get
        {
            return this.tipoDTEField;
        }
        set
        {
            this.tipoDTEField = value;
        }
    }

    /// <remarks/>
    public string Folio
    {
        get
        {
            return this.folioField;
        }
        set
        {
            this.folioField = value;
        }
    }
}

