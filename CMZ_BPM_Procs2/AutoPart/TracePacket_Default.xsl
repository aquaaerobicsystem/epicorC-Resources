<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="html"/>
<xsl:template match="/">
<html>
<head>
<meta name="keywords" content="TracePacket"/>
<meta name="description" content="Trace Packet Stylesheet"/>
<meta name="copyright" content="Copyright 2005 - Epicor Software, Corporation"/>
<meta name="designer" content="http://www.epicor.com"/>
<meta name="author" content="Ron Murphy"/>
<meta name="Parser" content="3.0"/>
<meta name="LastChange" content="10.06.2005 @17:30"/>

<style>
body { font-size: 8pt;  margin-top: 0px;  margin-left: 0px;  margin-right: 0px;  margin-bottom: 0px;  background-color: white;}
TABLE.workshop { width: 100%; display: block;  font-size: 8pt;  margin:15 0 15 0px;  border-collapse: collapse; empty-cells: show;}
TABLE.workshop TH {  display: block; padding:4 5 4 5px;  background-color:#f0f0f0;  vertical-align: top; text-align:left;}
TABLE.workshop TR {  display: block;   background-color: white; }
TABLE.workshop TD {  display: block;  padding:4 5 4 5px;  vertical-align: top;  font-family: "Ariel";}
TABLE.workshop .center { text-align: center;}
TABLE.workshop .right { text-align: right;}
TABLE.workshop TD.label {  background-color:#f0f0f0;padding-left:5px;}
.title {  font-size: 16pt;  padding:10 0 10 0px;margin:15 0 15 0px; text-align:center;}
.Link {  font-weight: normal;  font-size: 10pt;  color: blue;  text-decoration: underline;  cursor: hand;}
</style>

</head>
<form>
<body>
     <div class="title"><xsl:value-of select="/root/@name"/> - <xsl:value-of select="/root/@description"/></div>
    <xsl:variable name="section" select="generate-id()"/>

    <div class="Definition" style="margin-top:10px;">
      <div style="padding-bottom:20px;">
      <xsl:for-each select="//tracegroup">
             <a class="Link" style="padding-left:10px;" href="#Group_{@name}"><xsl:value-of select="@name"/></a>
        </xsl:for-each>
      </div>
      <xsl:apply-templates/>
    </div>
<br/>
</body>
</form>
</html>
</xsl:template>

<xsl:template match="/root/tracegroup">
    <xsl:variable name="groupkey">Group_<xsl:value-of select="@name"/></xsl:variable>
    <div id="{$groupkey}" class="title" style="text-align:left;background-color:silver;padding-left:10px;"><xsl:value-of select="@name"/></div>

    <div><a class="Link" href="#top" style="padding-left:10px;">Top</a>
        <xsl:for-each select="tracePacket">
             <xsl:variable name="key">#<xsl:value-of select="$groupkey"/>_<xsl:value-of select="position()"/></xsl:variable>
             <a href="{$key}" style="padding-left:10px;" class="Link"><xsl:value-of select="position()"/></a>
        </xsl:for-each>
    </div>
    
     <div>
    <table class="workshop" border="1" >
    <tr>
       <th>Nbr</th>
       <th><nobr>Thread ID</nobr></th>
       <th><nobr>Business Object</nobr></th>
       <th><nobr>Method Name</nobr></th>
       <th><nobr>AppServer Uri</nobr></th>
       <th><nobr>Return Type</nobr></th>
       <th><nobr>Execution Time</nobr></th>
       <th><nobr>Local Time</nobr></th>
       <th><nobr>Log Error</nobr></th>
       <th><nobr>Data Set Nbr</nobr></th>
    </tr>
   <xsl:apply-templates select="tracePacket"/>
    </table>
    </div>
</xsl:template>


<xsl:template match="tracePacket">
    <xsl:variable name="group">Group_<xsl:value-of select="../@name"/></xsl:variable>
    <xsl:variable name="pos"><xsl:value-of select="position()"/></xsl:variable>

    <tr>
       <td><a id="{$group}_{$pos}" href="#{$group}" class="Link"><xsl:value-of select="$pos"/></a></td>
      <td><nobr><xsl:value-of select="threadID"/></nobr></td>
       <td><xsl:value-of select="businessObject"/></td>
       <td><xsl:value-of select="methodName"/></td>
       <td><xsl:value-of select="appServerUri"/></td>
       <td><xsl:value-of select="returnType"/></td>
       <td class="right"><xsl:value-of select="executionTime"/></td>
       <td><nobr><xsl:value-of select="localTime"/></nobr></td>
       <td class="center"><xsl:value-of select="@LogError"/></td>
       <td class="center"><xsl:value-of select="@DataSetNbr"/></td>
  </tr>
   <xsl:apply-templates/>
</xsl:template>


<xsl:template match="parameters">
<tr>
    <td style="border-right:white 1px solid;" colspan="2"/>
    <td colspan="8">
    <table class="workshop" border="1" >
    <tr>
       <th><nobr>Parameter Name</nobr></th>
       <th><nobr>Type</nobr></th>
       <th><nobr>Data Set</nobr></th>
       <th><nobr>Value</nobr></th>
    </tr>
    <xsl:apply-templates/>
    </table>
    </td>
</tr>
</xsl:template>

<xsl:template match="parameter">
<tr>
    <td><xsl:value-of select="@name"/></td>
    <td><xsl:value-of select="@type"/></td>
    <td><xsl:value-of select="@dataSet"/></td>
    <td><xsl:value-of select="."/></td>
</tr>
</xsl:template>


<xsl:template match="paramDataSet">
<tr>
    <td style="border-right:white 1px solid;" colspan="2"/>
    <td colspan="8" >
    <table class="workshop" border="1" >
    <tr>
       <th><nobr>Table Name</nobr></th>
       <th><nobr>Row Number</nobr></th>
       <th><nobr>Column Name</nobr></th>
       <th><nobr>State</nobr></th>
       <th><nobr>Value</nobr></th>
    </tr>
    <xsl:apply-templates/>
    </table>
    </td>
</tr>
</xsl:template>

<xsl:template match="deltedRow">
<tr style="color:red;">
    <td><xsl:value-of select="@tableName"/></td>
    <td><xsl:value-of select="@rowNum"/></td>
    <td><xsl:value-of select="@colName"/></td>
    <td><xsl:value-of select="@rowState"/></td>
    <td><xsl:value-of select="."/></td>
</tr>
</xsl:template>

<xsl:template match="changedValue">
<tr>
    <td><xsl:value-of select="@tableName"/></td>
    <td><xsl:value-of select="@rowNum"/></td>
    <td><xsl:value-of select="@colName"/></td>
    <td><xsl:value-of select="@rowState"/></td>
    <td><xsl:value-of select="."/></td>
</tr>
</xsl:template>
<xsl:template match="businessObject | methodName | appServerUri | returnType |  localTime | executionTime"/>


  <xsl:template match="changedValue">
    <tr>
      <td>
        <xsl:value-of select="@tableName"/>
      </td>
      <td>
        <xsl:value-of select="@rowNum"/>
      </td>
      <td>
        <xsl:value-of select="@colName"/>
      </td>
      <td>
        <xsl:value-of select="@rowState"/>
      </td>
      <td>
        <xsl:value-of select="."/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="returnValues">
    <tr>
      <td style="border-right:white 1px solid;" colspan="2"/>
      <td colspan="8">
        <table class="workshop" border="1" >
          <tr>
            <th>
              <nobr>Return Parameter</nobr>
            </th>
            <th>
              <nobr>Type</nobr>
            </th>
            <th>
              <nobr>Data Set</nobr>
            </th>
            <th>
              <nobr>Value</nobr>
            </th>
          </tr>
          <xsl:apply-templates/>
        </table>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="returnParameter">
    <tr>
      <td>
        <xsl:value-of select="@name"/>
      </td>
      <td>
        <xsl:value-of select="@type"/>
      </td>
      <td>
        <xsl:value-of select="@dataSet"/>
      </td>
      <td>
        <xsl:value-of select="."/>
      </td>
    </tr>
  </xsl:template>



  <xsl:template match="Op">
    <tr>
      <td>
        <xsl:value-of select="@act"/>
      </td>
      <td>
        <xsl:value-of select="@dur"/>
      </td>
      <td>
        <xsl:value-of select="@machine"/>
      </td>
      <td>
        <xsl:value-of select="@pid"/>
      </td>
      <td>
        <!--<xsl:value-of select="."/>-->
        <xsl:for-each select="node()">
          <xsl:value-of select="name()"/>
          <xsl:for-each select="@*">

            <xsl:value-of select="name()"/>=
            "<xsl:value-of select="."/>"

          </xsl:for-each>
          <p>
            <xsl:value-of select="."></xsl:value-of>

          </p>
        </xsl:for-each>
      </td>
    </tr>
  </xsl:template>


  <xsl:template match="serverTrace">
    <tr>
      <td style="border-right:white 1px solid;" colspan="2"/>
      <td colspan="8" >
        <table class="workshop" border="1" >
          <tr>
            <th colspan="5">Server Trace</th>
          </tr>
          <tr>
            <th>
              <nobr>act</nobr>
            </th>
            <th>
              <nobr>dur</nobr>
            </th>
            <th>
              <nobr>machine</nobr>
            </th>
            <th>
              <nobr>pid</nobr>
            </th>
            <th>
              <nobr>Value</nobr>
            </th>
          </tr>
          <xsl:apply-templates/>
        </table>
      </td>
    </tr>
  </xsl:template>


</xsl:stylesheet>
