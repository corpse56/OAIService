using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

public partial class _Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //?verb=ListRecords&from=2015-03-05&metadataPrefix=marc21&until=2015-03-05
        string verb="";
        if (Request["verb"] != null)
        {
            verb = Request["verb"].ToLower();
        }
        if ((verb != "listrecords") && (verb != "getrecord") && (verb != "identify") && (verb != "listidentifiers"))
        {
            badVerb();
            return;
        }
        DateTime from,until;
        from = new DateTime(1, 1, 1);
        until = new DateTime(1,1,1);
        XmlDocument doc=new XmlDocument();

        switch (Request["verb"])
        {
            case "listrecords":
                if ((Request["from"] == null) && (Request["until"] == null))
                {
                    badArgument();
                    return;
                }
                if ((Request["from"] != null) && (Request["until"] == null))
                {
                    bool gooddate = DateTime.TryParse(Request["from"], out from);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                    until = DateTime.Today;
                }
                if ((Request["from"] == null) && (Request["until"] != null))
                {
                    bool gooddate = DateTime.TryParse(Request["until"], out until);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                    from = new DateTime(2008, 12, 31);
                }
                if ((Request["from"] != null) && (Request["until"] != null))
                {
                    bool gooddate = DateTime.TryParse(Request["from"], out from);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                    gooddate = DateTime.TryParse(Request["until"], out until);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                }
                if (from > until)
                {
                    badArgument();
                    return;
                }

                if (Request["metadataPrefix"] == null)
                {
                    badArgument();
                    return;
                }
                if (Request["metadataPrefix"] != "marc21")
                {
                    badArgument();
                }

                doc = ListRecords();
                break;
            default:

                //return x;
                break;
        }







        Response.Clear(); //Optional: if we've sent anything before
        Response.ContentType = "text/xml"; //Must be 'text/xml'
        Response.ContentEncoding = System.Text.Encoding.UTF8; //We'd like UTF-8
        doc.Save(Response.Output); //Save to the text-writer
        ////using the encoding of the text-writer
        ////(which comes from response.contentEncoding)
        Response.End(); //Optional: will end processing
    }



    private void badArgument()
    {
            //<?xml version="1.0" encoding="UTF-8"?>
            //<OAI-PMH xmlns="http://www.openarchives.org/OAI/2.0/" 
            //         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
            //         xsi:schemaLocation="http://www.openarchives.org/OAI/2.0/
            //         http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd">
            //  <responseDate>2002-06-01T19:20:30Z</responseDate> 
            //  <request verb="ListRecords" from="2002-06-01T02:00:00Z"
            //           until="2002-06-01T03:020:00Z"
            //           metadataPrefix="oai_marc">
            //           http://memory.loc.gov/cgi-bin/oai</request>
            //  <error code="badArgument"/>
            //</OAI-PMH>    
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("OAI-PMH");
        XmlAttribute attribute = xmlDoc.CreateAttribute("xmlns");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xmlns:xsi");
        attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/ http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd";
        rootNode.Attributes.Append(attribute);
        xmlDoc.AppendChild(rootNode);
        XmlNode node = xmlDoc.CreateElement("responseDate");
        node.InnerText = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        rootNode.AppendChild(node);
        node = xmlDoc.CreateElement("request");
        node.InnerText = Request.Url.AbsoluteUri;
        rootNode.AppendChild(node);
        node = xmlDoc.CreateElement("error");
        attribute = xmlDoc.CreateAttribute("code");
        attribute.Value = "badArgument";
        node.Attributes.Append(attribute);
        rootNode.AppendChild(node);

        Response.Clear();
        Response.ContentType = "text/xml";
        Response.ContentEncoding = System.Text.Encoding.UTF8;
        xmlDoc.Save(Response.Output);
        Response.End(); 
    }

    private void badVerb()
    {
        //<?xml version="1.0" encoding="UTF-8"?>
        //<OAI-PMH xmlns="http://www.openarchives.org/OAI/2.0/" 
        //         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        //         xsi:schemaLocation="http://www.openarchives.org/OAI/2.0/
        //         http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd">
        //  <responseDate>2002-05-01T09:18:29Z</responseDate>
        //  <request>http://arXiv.org/oai2</request>
        //  <error code="badVerb">Illegal OAI verb</error>
        //</OAI-PMH>
        
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("OAI-PMH");
        XmlAttribute attribute = xmlDoc.CreateAttribute("xmlns");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xmlns:xsi");
        attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/ http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd";
        rootNode.Attributes.Append(attribute);
        xmlDoc.AppendChild(rootNode);
        XmlNode node = xmlDoc.CreateElement("responseDate");
        node.InnerText = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        rootNode.AppendChild(node);
        node = xmlDoc.CreateElement("request");
        node.InnerText = Request.Url.AbsoluteUri;
        rootNode.AppendChild(node);
        node = xmlDoc.CreateElement("error");
        attribute = xmlDoc.CreateAttribute("code");
        attribute.Value = "badVerb";
        node.InnerText = "Illegal OAI verb";
        rootNode.AppendChild(node);

        Response.Clear(); 
        Response.ContentType = "text/xml"; 
        Response.ContentEncoding = System.Text.Encoding.UTF8;
        xmlDoc.Save(Response.Output); 
        Response.End(); 
        //return xmlDoc;

    }
    private XmlDocument ListRecords()
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("OAI-PMH");
        XmlAttribute attribute = xmlDoc.CreateAttribute("xmlns");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xmlns:xsi");
        attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/ http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd";
        rootNode.Attributes.Append(attribute);
        xmlDoc.AppendChild(rootNode);
        XmlNode node = xmlDoc.CreateElement("responseDate");
        node.InnerText = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        rootNode.AppendChild(node);

        node = xmlDoc.CreateElement("request");
        attribute = xmlDoc.CreateAttribute("verb");
        attribute.Value = Request["verb"];
        node.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("from");
        attribute.Value = Request["from"];
        node.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("until");
        attribute.Value = Request["until"];
        node.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("metadataPrefix");
        attribute.Value = Request["metadataPrefix"];
        node.Attributes.Append(attribute);
        rootNode.AppendChild(node);

        XmlNode ListRecords = xmlDoc.CreateElement("ListRecords");
        rootNode.AppendChild(ListRecords);
        node = xmlDoc.CreateElement("record");
        ListRecords.AppendChild(node);
        node = xmlDoc.CreateElement("header");




        //<request verb="ListRecords" from="2015-03-05" until="2015-03-05" metadataPrefix="marc21">http://aleph.nlr.ru/OAI</request>
        //<request metadataPrefix="marc21" from="2015-04-14" until="2015-05-31" verb="ListRecords">http://aleph.nlr.ru/OAI</request>

        return xmlDoc;
    }

}
