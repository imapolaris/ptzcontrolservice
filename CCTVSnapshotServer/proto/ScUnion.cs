//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: ScUnion.proto
namespace Adapter.Proto
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ScUnion")]
  public partial class ScUnion : global::ProtoBuf.IExtensible
  {
    public ScUnion() {}
    
    private string _ID;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"ID", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string ID
    {
      get { return _ID; }
      set { _ID = value; }
    }
    private double _Latitude;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"Latitude", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public double Latitude
    {
      get { return _Latitude; }
      set { _Latitude = value; }
    }
    private double _Longitude;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"Longitude", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public double Longitude
    {
      get { return _Longitude; }
      set { _Longitude = value; }
    }
    private float _COG;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"COG", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    public float COG
    {
      get { return _COG; }
      set { _COG = value; }
    }
    private float _SOG;
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"SOG", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    public float SOG
    {
      get { return _SOG; }
      set { _SOG = value; }
    }
    private float _ROT = default(float);
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"ROT", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(float))]
    public float ROT
    {
      get { return _ROT; }
      set { _ROT = value; }
    }
    private float _TrueHeading = (float)511;
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"TrueHeading", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue((float)511)]
    public float TrueHeading
    {
      get { return _TrueHeading; }
      set { _TrueHeading = value; }
    }
    private int _MMSI = default(int);
    [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name=@"MMSI", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int MMSI
    {
      get { return _MMSI; }
      set { _MMSI = value; }
    }
    private int _NavStatus = (int)15;
    [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name=@"NavStatus", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue((int)15)]
    public int NavStatus
    {
      get { return _NavStatus; }
      set { _NavStatus = value; }
    }
    private long _DynamicTime;
    [global::ProtoBuf.ProtoMember(10, IsRequired = true, Name=@"DynamicTime", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    public long DynamicTime
    {
      get { return _DynamicTime; }
      set { _DynamicTime = value; }
    }
    private readonly global::System.Collections.Generic.List<string> _Origins = new global::System.Collections.Generic.List<string>();
    [global::ProtoBuf.ProtoMember(11, Name=@"Origins", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<string> Origins
    {
      get { return _Origins; }
    }
  
    private int _Timeout = (int)600;
    [global::ProtoBuf.ProtoMember(13, IsRequired = false, Name=@"Timeout", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue((int)600)]
    public int Timeout
    {
      get { return _Timeout; }
      set { _Timeout = value; }
    }
    private long _StaticTime = default(long);
    [global::ProtoBuf.ProtoMember(30, IsRequired = false, Name=@"StaticTime", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(long))]
    public long StaticTime
    {
      get { return _StaticTime; }
      set { _StaticTime = value; }
    }
    private string _Name = "";
    [global::ProtoBuf.ProtoMember(31, IsRequired = false, Name=@"Name", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Name
    {
      get { return _Name; }
      set { _Name = value; }
    }
    private string _CallSign = "";
    [global::ProtoBuf.ProtoMember(32, IsRequired = false, Name=@"CallSign", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string CallSign
    {
      get { return _CallSign; }
      set { _CallSign = value; }
    }
    private int _IMO = default(int);
    [global::ProtoBuf.ProtoMember(33, IsRequired = false, Name=@"IMO", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int IMO
    {
      get { return _IMO; }
      set { _IMO = value; }
    }
    private long _ETA = default(long);
    [global::ProtoBuf.ProtoMember(34, IsRequired = false, Name=@"ETA", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(long))]
    public long ETA
    {
      get { return _ETA; }
      set { _ETA = value; }
    }
    private string _Destination = "";
    [global::ProtoBuf.ProtoMember(35, IsRequired = false, Name=@"Destination", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Destination
    {
      get { return _Destination; }
      set { _Destination = value; }
    }
    private float _Draught = default(float);
    [global::ProtoBuf.ProtoMember(36, IsRequired = false, Name=@"Draught", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(float))]
    public float Draught
    {
      get { return _Draught; }
      set { _Draught = value; }
    }
    private int _Length = default(int);
    [global::ProtoBuf.ProtoMember(37, IsRequired = false, Name=@"Length", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int Length
    {
      get { return _Length; }
      set { _Length = value; }
    }
    private int _Width = default(int);
    [global::ProtoBuf.ProtoMember(38, IsRequired = false, Name=@"Width", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int Width
    {
      get { return _Width; }
      set { _Width = value; }
    }
    private int _RefToProw = default(int);
    [global::ProtoBuf.ProtoMember(39, IsRequired = false, Name=@"RefToProw", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int RefToProw
    {
      get { return _RefToProw; }
      set { _RefToProw = value; }
    }
    private int _RefToLarboard = default(int);
    [global::ProtoBuf.ProtoMember(40, IsRequired = false, Name=@"RefToLarboard", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int RefToLarboard
    {
      get { return _RefToLarboard; }
      set { _RefToLarboard = value; }
    }
    private int _ShipCargoType = default(int);
    [global::ProtoBuf.ProtoMember(41, IsRequired = false, Name=@"ShipCargoType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int ShipCargoType
    {
      get { return _ShipCargoType; }
      set { _ShipCargoType = value; }
    }
    private int _MeasureA = default(int);
    [global::ProtoBuf.ProtoMember(42, IsRequired = false, Name=@"MeasureA", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int MeasureA
    {
      get { return _MeasureA; }
      set { _MeasureA = value; }
    }
    private int _MeasureC = default(int);
    [global::ProtoBuf.ProtoMember(43, IsRequired = false, Name=@"MeasureC", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int MeasureC
    {
      get { return _MeasureC; }
      set { _MeasureC = value; }
    }
    private long _IMEI = default(long);
    [global::ProtoBuf.ProtoMember(51, IsRequired = false, Name=@"IMEI", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(long))]
    public long IMEI
    {
      get { return _IMEI; }
      set { _IMEI = value; }
    }
    private string _SIM = "";
    [global::ProtoBuf.ProtoMember(52, IsRequired = false, Name=@"SIM", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string SIM
    {
      get { return _SIM; }
      set { _SIM = value; }
    }
    private string _V_Name = "";
    [global::ProtoBuf.ProtoMember(70, IsRequired = false, Name=@"V_Name", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string V_Name
    {
      get { return _V_Name; }
      set { _V_Name = value; }
    }
    private int _V_ShipCargoType = default(int);
    [global::ProtoBuf.ProtoMember(71, IsRequired = false, Name=@"V_ShipCargoType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int V_ShipCargoType
    {
      get { return _V_ShipCargoType; }
      set { _V_ShipCargoType = value; }
    }
    private float _V_Length = default(float);
    [global::ProtoBuf.ProtoMember(72, IsRequired = false, Name=@"V_Length", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(float))]
    public float V_Length
    {
      get { return _V_Length; }
      set { _V_Length = value; }
    }
    private float _V_Width = default(float);
    [global::ProtoBuf.ProtoMember(73, IsRequired = false, Name=@"V_Width", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
    [global::System.ComponentModel.DefaultValue(default(float))]
    public float V_Width
    {
      get { return _V_Width; }
      set { _V_Width = value; }
    }
    private string _V_VesselId = "";
    [global::ProtoBuf.ProtoMember(74, IsRequired = false, Name=@"V_VesselId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string V_VesselId
    {
      get { return _V_VesselId; }
      set { _V_VesselId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}