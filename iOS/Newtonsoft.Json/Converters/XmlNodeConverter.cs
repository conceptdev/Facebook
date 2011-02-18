#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Utilities;
using System.Linq;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Converts an <see cref="XmlNode"/> to and from JSON.
  /// </summary>
  public class XmlNodeConverter : JsonConverter
  {
    private const string TextName = "#text";
    private const string CommentName = "#comment";
    private const string CDataName = "#cdata-section";
    private const string WhitespaceName = "#whitespace";
    private const string SignificantWhitespaceName = "#significant-whitespace";
    private const string DeclarationName = "?xml";
    private const string JsonNamespaceUri = "http://james.newtonking.com/projects/json";

    /// <summary>
    /// Gets or sets the name of the root element to insert when deserializing to XML if the JSON structure has produces multiple root elements.
    /// </summary>
    /// <value>The name of the deserialize root element.</value>
    public string DeserializeRootElementName { get; set; }

    #region Writing
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <param name="value">The value.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      XmlNode node = value as XmlNode;

      if (node == null)
        throw new ArgumentException("Value must be an XmlNode", "value");

      writer.WriteStartObject();
      SerializeNode(writer, node, true);
      writer.WriteEndObject();
    }

    private string GetPropertyName(XmlNode node)
    {
      switch (node.NodeType)
      {
        case XmlNodeType.Attribute:
          return "@" + node.Name;
        case XmlNodeType.CDATA:
          return CDataName;
        case XmlNodeType.Comment:
          return CommentName;
        case XmlNodeType.Element:
          return node.Name;
        case XmlNodeType.ProcessingInstruction:
          return "?" + node.Name;
        case XmlNodeType.XmlDeclaration:
          return DeclarationName;
        case XmlNodeType.SignificantWhitespace:
          return SignificantWhitespaceName;
        case XmlNodeType.Text:
          return TextName;
        case XmlNodeType.Whitespace:
          return WhitespaceName;
        default:
          throw new JsonSerializationException("Unexpected XmlNodeType when getting node name: " + node.NodeType);
      }
    }

    private void SerializeGroupedNodes(JsonWriter writer, XmlNode node)
    {
      // group nodes together by name
      Dictionary<string, List<XmlNode>> nodesGroupedByName = new Dictionary<string, List<XmlNode>>();

      for (int i = 0; i < node.ChildNodes.Count; i++)
      {
        XmlNode childNode = node.ChildNodes[i];
        string nodeName = GetPropertyName(childNode);

        List<XmlNode> nodes;
        if (!nodesGroupedByName.TryGetValue(nodeName, out nodes))
        {
          nodes = new List<XmlNode>();
          nodesGroupedByName.Add(nodeName, nodes);
        }

        nodes.Add(childNode);
      }

      // loop through grouped nodes. write single name instances as normal,
      // write multiple names together in an array
      foreach (KeyValuePair<string, List<XmlNode>> nodeNameGroup in nodesGroupedByName)
      {
        List<XmlNode> groupedNodes = nodeNameGroup.Value;
        bool writeArray;

        if (groupedNodes.Count == 1)
        {
          XmlNode singleNode = groupedNodes[0];
          XmlAttribute jsonArrayAttribute = (singleNode.Attributes != null) ? singleNode.Attributes["Array", JsonNamespaceUri] : null;
          if (jsonArrayAttribute != null)
            writeArray = XmlConvert.ToBoolean(jsonArrayAttribute.Value);
          else
            writeArray = false;
        }
        else
        {
          writeArray = true;
        }

        if (!writeArray)
        {
          SerializeNode(writer, groupedNodes[0], true);
        }
        else
        {
          string elementNames = nodeNameGroup.Key;
          writer.WritePropertyName(nodeNameGroup.Key);
          writer.WriteStartArray();

          for (int i = 0; i < groupedNodes.Count; i++)
          {
            SerializeNode(writer, groupedNodes[i], false);
          }

          writer.WriteEndArray();
        }
      }
    }

    private void SerializeNode(JsonWriter writer, XmlNode node, bool writePropertyName)
    {
      switch (node.NodeType)
      {
        case XmlNodeType.Document:
        case XmlNodeType.DocumentFragment:
          SerializeGroupedNodes(writer, node);
          break;
        case XmlNodeType.Element:
          if (writePropertyName)
            writer.WritePropertyName(node.Name);

          if (ValueAttributes(node.Attributes).Count() == 0 && node.ChildNodes.Count == 1
                  && node.ChildNodes[0].NodeType == XmlNodeType.Text)
          {
            // write elements with a single text child as a name value pair
            writer.WriteValue(node.ChildNodes[0].Value);
          }
          else if (node.ChildNodes.Count == 0 && CollectionUtils.IsNullOrEmpty(node.Attributes))
          {
            // empty element
            writer.WriteNull();
          }
          else if (node.ChildNodes.OfType<XmlElement>().Where(x => x.Name.StartsWith("-")).Count() > 1)
          {
            XmlElement constructorValueElement = node.ChildNodes.OfType<XmlElement>().Where(x => x.Name.StartsWith("-")).First();
            string constructorName = constructorValueElement.Name.Substring(1);

            writer.WriteStartConstructor(constructorName);

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
              SerializeNode(writer, node.ChildNodes[i], false);
            }

            writer.WriteEndConstructor();
          }
          else
          {
            writer.WriteStartObject();

            for (int i = 0; i < node.Attributes.Count; i++)
            {
              SerializeNode(writer, node.Attributes[i], true);
            }

            SerializeGroupedNodes(writer, node);

            writer.WriteEndObject();
          }

          break;
        case XmlNodeType.Comment:
          if (writePropertyName)
            writer.WriteComment(node.Value);
          break;
        case XmlNodeType.Attribute:
        case XmlNodeType.Text:
        case XmlNodeType.CDATA:
        case XmlNodeType.ProcessingInstruction:
        case XmlNodeType.Whitespace:
        case XmlNodeType.SignificantWhitespace:
          if (node.Prefix == "xmlns" && node.Value == JsonNamespaceUri)
            break;
          else if (node.NamespaceURI == JsonNamespaceUri)
            break;

          if (writePropertyName)
            writer.WritePropertyName(GetPropertyName(node));
          writer.WriteValue(node.Value);
          break;
        case XmlNodeType.XmlDeclaration:
          XmlDeclaration declaration = (XmlDeclaration)node;
          writer.WritePropertyName(GetPropertyName(node));
          writer.WriteStartObject();

          if (!string.IsNullOrEmpty(declaration.Version))
          {
            writer.WritePropertyName("@version");
            writer.WriteValue(declaration.Version);
          }
          if (!string.IsNullOrEmpty(declaration.Encoding))
          {
            writer.WritePropertyName("@encoding");
            writer.WriteValue(declaration.Encoding);
          }
          if (!string.IsNullOrEmpty(declaration.Standalone))
          {
            writer.WritePropertyName("@standalone");
            writer.WriteValue(declaration.Standalone);
          }

          writer.WriteEndObject();
          break;
        default:
          throw new JsonSerializationException("Unexpected XmlNodeType when serializing nodes: " + node.NodeType);
      }
    }
    #endregion

    #region Reading
    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, JsonSerializer serializer)
    {
      // maybe have CanReader and a CanWrite methods so this sort of test wouldn't be necessary
      if (objectType != typeof(XmlDocument))
        throw new JsonSerializationException("XmlNodeConverter only supports deserializing XmlDocuments");

      XmlDocument document = new XmlDocument();
      XmlNamespaceManager manager = new XmlNamespaceManager(document.NameTable);

      XmlNode rootNode;
      
      if (!string.IsNullOrEmpty(DeserializeRootElementName))
      {
        rootNode = document.CreateElement(DeserializeRootElementName);
        document.AppendChild(rootNode);
      }
      else
      {
        rootNode = document;
      }

      if (reader.TokenType != JsonToken.StartObject)
        throw new JsonSerializationException("XmlNodeConverter can only convert JSON that begins with an object.");

      reader.Read();

      DeserializeNode(reader, document, manager, rootNode);

      return document;
    }

    private void DeserializeValue(JsonReader reader, XmlDocument document, XmlNamespaceManager manager, string propertyName, XmlNode currentNode)
    {
      switch (propertyName)
      {
        case TextName:
          currentNode.AppendChild(document.CreateTextNode(reader.Value.ToString()));
          break;
        case CDataName:
          currentNode.AppendChild(document.CreateCDataSection(reader.Value.ToString()));
          break;
        case WhitespaceName:
          currentNode.AppendChild(document.CreateWhitespace(reader.Value.ToString()));
          break;
        case SignificantWhitespaceName:
          currentNode.AppendChild(document.CreateSignificantWhitespace(reader.Value.ToString()));
          break;
        default:
          // processing instructions and the xml declaration start with ?
          if (!string.IsNullOrEmpty(propertyName) && propertyName[0] == '?')
          {
            if (propertyName == DeclarationName)
            {
              string version = null;
              string encoding = null;
              string standalone = null;
              while (reader.Read() && reader.TokenType != JsonToken.EndObject)
              {
                switch (reader.Value.ToString())
                {
                  case "@version":
                    reader.Read();
                    version = reader.Value.ToString();
                    break;
                  case "@encoding":
                    reader.Read();
                    encoding = reader.Value.ToString();
                    break;
                  case "@standalone":
                    reader.Read();
                    standalone = reader.Value.ToString();
                    break;
                  default:
                    throw new JsonSerializationException("Unexpected property name encountered while deserializing XmlDeclaration: " + reader.Value);
                }
              }

              XmlDeclaration declaration = document.CreateXmlDeclaration(version, encoding, standalone);
              currentNode.AppendChild(declaration);
            }
            else
            {
              XmlProcessingInstruction instruction = document.CreateProcessingInstruction(propertyName.Substring(1), reader.Value.ToString());
              currentNode.AppendChild(instruction);
            }
          }
          else
          {
            // deserialize xml element
            bool finishedAttributes = false;
            bool finishedElement = false;
            string elementPrefix = GetPrefix(propertyName);
            Dictionary<string, string> attributeNameValues = new Dictionary<string, string>();

            if (reader.TokenType == JsonToken.StartArray)
            {
              XmlElement nestedArrayElement = CreateElement(propertyName, document, elementPrefix, manager);

              currentNode.AppendChild(nestedArrayElement);

              while (reader.Read() && reader.TokenType != JsonToken.EndArray)
              {
                DeserializeValue(reader, document, manager, propertyName, nestedArrayElement);
              }

              return;
            }

            // a string token means the element only has a single text child
            if (reader.TokenType != JsonToken.String
              && reader.TokenType != JsonToken.Null
              && reader.TokenType != JsonToken.Boolean
              && reader.TokenType != JsonToken.Integer
              && reader.TokenType != JsonToken.Float
              && reader.TokenType != JsonToken.Date
              && reader.TokenType != JsonToken.StartConstructor)
            {
              // read properties until first non-attribute is encountered
              while (!finishedAttributes && !finishedElement && reader.Read())
              {
                switch (reader.TokenType)
                {
                  case JsonToken.PropertyName:
                    string attributeName = reader.Value.ToString();

                    if (attributeName[0] == '@')
                    {
                      attributeName = attributeName.Substring(1);
                      reader.Read();
                      string attributeValue = reader.Value.ToString();
                      attributeNameValues.Add(attributeName, attributeValue);

                      string namespacePrefix;

                      if (IsNamespaceAttribute(attributeName, out namespacePrefix))
                      {
                        manager.AddNamespace(namespacePrefix, attributeValue);
                      }
                    }
                    else
                    {
                      finishedAttributes = true;
                    }
                    break;
                  case JsonToken.EndObject:
                    finishedElement = true;
                    break;
                  default:
                    throw new JsonSerializationException("Unexpected JsonToken: " + reader.TokenType);
                }
              }
            }

            // have to wait until attributes have been parsed before creating element
            // attributes may contain namespace info used by the element
            XmlElement element = CreateElement(propertyName, document, elementPrefix, manager);

            currentNode.AppendChild(element);

            // add attributes to newly created element
            foreach (KeyValuePair<string, string> nameValue in attributeNameValues)
            {
              string attributePrefix = GetPrefix(nameValue.Key);

              XmlAttribute attribute = (!string.IsNullOrEmpty(attributePrefix))
                      ? document.CreateAttribute(nameValue.Key, manager.LookupNamespace(attributePrefix))
                      : document.CreateAttribute(nameValue.Key);

              attribute.Value = nameValue.Value;

              element.SetAttributeNode(attribute);
            }

            if (reader.TokenType == JsonToken.String)
            {
              element.AppendChild(document.CreateTextNode(reader.Value.ToString()));
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
              element.AppendChild(document.CreateTextNode(XmlConvert.ToString((long)reader.Value)));
            }
            else if (reader.TokenType == JsonToken.Float)
            {
              element.AppendChild(document.CreateTextNode(XmlConvert.ToString((double)reader.Value)));
            }
            else if (reader.TokenType == JsonToken.Boolean)
            {
              element.AppendChild(document.CreateTextNode(XmlConvert.ToString((bool)reader.Value)));
            }
            else if (reader.TokenType == JsonToken.Date)
            {
              DateTime d = (DateTime)reader.Value;
              element.AppendChild(document.CreateTextNode(XmlConvert.ToString(d, DateTimeUtils.ToSerializationMode(d.Kind))));
            }
            else if (reader.TokenType == JsonToken.Null)
            {
              // empty element. do nothing
            }
            else
            {
              // finished element will have no children to deserialize
              if (!finishedElement)
              {
                manager.PushScope();

                DeserializeNode(reader, document, manager, element);

                manager.PopScope();
              }
            }
          }
          break;
      }
    }

    private XmlElement CreateElement(string elementName, XmlDocument document, string elementPrefix, XmlNamespaceManager manager)
    {
      return (!string.IsNullOrEmpty(elementPrefix))
               ? document.CreateElement(elementName, manager.LookupNamespace(elementPrefix))
               : document.CreateElement(elementName);
    }

    private void DeserializeNode(JsonReader reader, XmlDocument document, XmlNamespaceManager manager, XmlNode currentNode)
    {
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            if (currentNode.NodeType == XmlNodeType.Document && document.DocumentElement != null)
              throw new JsonSerializationException("JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifing a DeserializeRootElementName.");

            string propertyName = reader.Value.ToString();
            reader.Read();

            if (reader.TokenType == JsonToken.StartArray)
            {
              while (reader.Read() && reader.TokenType != JsonToken.EndArray)
              {
                DeserializeValue(reader, document, manager, propertyName, currentNode);
              }
            }
            else
            {
              DeserializeValue(reader, document, manager, propertyName, currentNode);
            }
            break;
          case JsonToken.StartConstructor:
            string constructorName = reader.Value.ToString();

            while (reader.Read() && reader.TokenType != JsonToken.EndConstructor)
            {
              DeserializeValue(reader, document, manager, "-" + constructorName, currentNode);
            }
            break;
          case JsonToken.EndObject:
          case JsonToken.EndArray:
            return;
          default:
            throw new JsonSerializationException("Unexpected JsonToken when deserializing node: " + reader.TokenType);
        }
      } while (reader.TokenType == JsonToken.PropertyName || reader.Read());
      // don't read if current token is a property. token was already read when parsing element attributes
    }

    /// <summary>
    /// Checks if the attributeName is a namespace attribute.
    /// </summary>
    /// <param name="attributeName">Attribute name to test.</param>
    /// <param name="prefix">The attribute name prefix if it has one, otherwise an empty string.</param>
    /// <returns>True if attribute name is for a namespace attribute, otherwise false.</returns>
    private bool IsNamespaceAttribute(string attributeName, out string prefix)
    {
      if (attributeName.StartsWith("xmlns", StringComparison.Ordinal))
      {
        if (attributeName.Length == 5)
        {
          prefix = string.Empty;
          return true;
        }
        else if (attributeName[5] == ':')
        {
          prefix = attributeName.Substring(6, attributeName.Length - 6);
          return true;
        }
      }
      prefix = null;
      return false;
    }

    private string GetPrefix(string qualifiedName)
    {
      int colonPosition = qualifiedName.IndexOf(':');

      if ((colonPosition == -1 || colonPosition == 0) || (qualifiedName.Length - 1) == colonPosition)
        return string.Empty;
      else
        return qualifiedName.Substring(0, colonPosition);
    }

    private IEnumerable<XmlAttribute> ValueAttributes(XmlAttributeCollection c)
    {
      return c.OfType<XmlAttribute>().Where(a => a.NamespaceURI != JsonNamespaceUri);
    }

    private IEnumerable<XmlNode> ValueNodes(XmlNodeList c)
    {
      return c.OfType<XmlNode>().Where(n => n.NamespaceURI != JsonNamespaceUri);
    }
    #endregion

    /// <summary>
    /// Determines whether this instance can convert the specified value type.
    /// </summary>
    /// <param name="valueType">Type of the value.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type valueType)
    {
      return typeof(XmlNode).IsAssignableFrom(valueType);
    }
  }
}
#endif