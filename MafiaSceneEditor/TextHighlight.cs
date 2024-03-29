﻿using System.Collections.Generic;

namespace YAMSE
{
    public static class MafiaKeywords
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
            "act_setstate",
            "actor_duplicate",
            "actor_setdir",
            "actor_setplacement",
            "autosavegamefull",
            "autosavegame",
            "callsubroutine",
            "camera_lock",
            "camera_unlock",
            "car_calm",
            "car_enableus",
            "car_explosion",
            "car_forcestop",
            "car_getspeed",
            "car_invisible",
            "car_lock_all",
            "car_muststeal",
            "car_repair",
            "car_setaxis",
            "car_setprojector",
            "car_setspeccoll",
            "car_setspeed",
            "car_switchshowenergy",
            "car_unbreakable",
            "cardamagevisible",
            "citycaching_tick",
            "citymusic_off",
            "cleardifferences",
            "commandblock",
            "compareactors",
            "compareownerwith",
            "compareownerwithex",
            "console_addtext",
            "console_addtext",
            "create_physicalobject",
            "createweaponfromframe",
            "ctrl_read",
            "debug_text",
            "detector_inrange",
            "detector_issignal",
            "detector_setsignal",
            "detector_waitforhit",
            "detector_waitforuse",
            "dialog_begin",
            "dialog_camswitch",
            "dialog_end",
            "disablecolls",
            "door_enableus",
            "door_lock",
            "door_open",
            "down",
            "emitparticle",
            "enablemap",
            "endofmission",
            "end",
            "enemy_action_fire",
            "enemy_action_follow",
            "enemy_actionsclear",
            "enemy_block",
            "enemy_brainwash",
            "enemy_car_moveto",
            "enemy_followplayer",
            "enemy_forcescript",
            "enemy_group_add",
            "enemy_group_new",
            "enemy_changeanim",
            "enemy_lockstate",
            "enemy_look",
            "enemy_lookto",
            "enemy_move",
            "enemy_move_to_frame",
            "enemy_playanim",
            "enemy_stopanim",
            "enemy_talk",
            "enemy_unblock",
            "enemy_use_railway",
            "enemy_usecar",
            "enemy_vidim",
            "enemy_wait",
            "event",
            "event_use_cb",
            "explosion",
            "findactor",
            "findframe",
            "findnearactor",
            "freeride_enablecar",
            "freeride_scoreadd",
            "freeride_scoreget",
            "freeride_scoreon",
            "freeride_scoreset",
            "freeride_traffsetup",
            "frm_getpos",
            "frm_seton",
            "frm_setpos",
            "fuckingbox_add",
            "fuckingbox_add_dest",
            "fuckingbox_getnumdest",
            "fuckingbox_recompile",
            "game_nightmission",
            "garage_addcar",
            "garage_carmanager",
            "garage_enablesteal",
            "garage_generatecars",
            "garage_nezahazuj",
            "get_remote_actor",
            "getactiveplayer",
            "getactorframe",
            "getactorsdist",
            "getangleactortoactor",
            "getcardamage",
            "getenemyaistate",
            "getgametime",
            "getframefromactor",
            "getmissionnumber",
            "getticktime",
            "gosub",
            "human_activateweapon",
            "human_addweapon",
            "human_canaddweapon",
            "human_candie",
            "human_createab",
            "human_death",
            "human_eraseab",
            "human_force_settocar",
            "human_forcefall",
            "human_getowner",
            "human_getproperty",
            "human_getproperty",
            "human_havebox",
            "human_holster",
            "human_changemodel",
            "human_isweapon",
            "human_lockotactor",
            "human_looktoactor",
            "human_lookto",
            "human_set8slot",
            "human_setproperty",
            "human_talk",
            "human_throwitem",
            "change_mission",
            "character_pop",
            "iffltinrange",
            "ifplayerstealcar",
            "inventory_pop",
            "iscarusable",
            "loaddifferences",
            "mission_objectivesclear",
            "mission_objectivesremove",
            "mission_objectives",
            "person_playanim",
            "person_stopanim",
            "player_lockcontrols",
            "playsound",
            "police_speed_factor",
            "policeitchforplayer",
            "policemanager_forcearrest",
            "policemanager_on",
            "policemanager_del",
            "policemanager_add",
            "recloadfull",
            "recunload",
            "recwaitforend",
            "return",
            "rnd",
            "set_remote_float",
            "setcitytrafficvisible",
            "setcompass",
            "setevent",
            "setmissionnumber",
            "setnoanimhit",
            "setnpckillevent",
            "setplayerhornevent",
            "settimeoutevent",
            "SetTraffSectorSnd",
            "sound_setvolume",
            "stopparticle",
            "stream_create",
            "stream_destroy",
            "stream_fadevol",
            "stream_play",
            "stream_setloop",
            "stream_stop",
            "subtitle_add",
            "taxidriver_enable",
            "timeroff",
            "timeron",
            "version_is_germany",
            "wait",
            "weather_reset",
            "weather_setparam",
            "wingman_delindicator",
            "wingman_setindicator",
            "zatmyse",
        };
    }
}
