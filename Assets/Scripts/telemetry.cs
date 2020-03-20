using System;
using System.Collections;
using System.Collections.Generic;


[Serializable]
public class telemetry
{
    public List<telemetry_data> telem;
}

[Serializable]
public class telemetry_data
{
    public string heart_bpm;
    public string p_sub;
    public string p_suit;
    public string t_sub;
    public string v_fan;
    public string p_o2;
    public string rate_o2;
    public string cap_battery;
    public string p_h2o_g;
    public string p_h2o_l;
    public string p_sop;
    public string rate_sop;
    public string t_battery;
    public string t_oxygen;
    public string t_water;
    public string create_date;
}
