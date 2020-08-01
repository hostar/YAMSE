using System.Collections.Generic;

namespace YAMSE
{
    public static class TextHighlight
    {
        public static string Comment = "//";
        public static List<string> Keywords = new List<string>
         {
            "if",
            "goto",
            "label",
            "let",
        };
        public static List<string> Declaration = new List<string>
         {
            "dim_act",
            "dim_flt",
            "dim_frm",
        };
        public static List<string> Commands = new List<string>
         {
            "findactor",
            "detector_waitforuse",
            "human_canaddweapon",
            "freeride_scoreget",
            "freeride_scoreset",
            "human_addweapon",
            "console_addtext",
            "human_isweapon",
            "findnearactor",
            "freeride_scoreadd",
            "setevent",
            "act_setstate",
            "wait",
            "findframe",
            "frm_getpos",
            "actor_setplacement",
            "explosion",
            "rnd",
            "end",
            "autosavegamefull",
            "setmissionnumber",
            "commandblock",
            "player_lockcontrols",
            "zatmyse",
            "subtitle_add",
            "event",
            "camera_lock",
            "mission_objectivesclear",
            "person_playanim",
            "car_repair",
            "console_addtext",
            "camera_unlock",
            "setcitytrafficvisible",
            "detector_inrange",
            "policeitchforplayer",
            "human_talk",
            "compareownerwithex",
            "human_getowner",
            "car_forcestop",
            "car_calm",
            "getticktime",
            "frm_setpos",
            "getactorsdist",
            "dialog_begin",
            "dialog_camswitch",
            "dialog_end",
            "actor_duplicate",
            "actor_setdir",
            "frm_seton",
            "person_stopanim",
            "citycaching_tick",
            "settimeoutevent",
            "timeron",
            "down",
            "timeroff",
            "setcompass",
            "car_enableus",
            "detector_setsignal",
            "mission_objectives",
            "freeride_enablecar",
        };
    }
}
