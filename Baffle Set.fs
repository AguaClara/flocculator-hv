FeatureScript 1589;
import(path : "onshape/std/geometry.fs", version : "1589.0");

import(path : "c2f4584cf9d8b1114f7ff5b4", version : "500038aece54ddfb3afc1081");


export const bafflesetTree =
{
        name : "baffleset",
        notes : {
            description : "",
            imagelink : "",
            textbooklink : "",
        },
        designers : {
            pre : bafflePreDesigner,
            post : bafflePostDesigner,
        },
        params : {
            rep : true,
            ip : "app",
            flowfront : false, //if water comes in at the bottom and moves from to back, hitting the baffles at the front
            lastchannel : false,
            tankH : [1, 2, 200], //height of tank, will be defined in parent via tank info
            channelW : [0.05, 0.5, 100], //width of channel, will be defined in parent via tank info
            channelL : [0, 7, 200], //length of channel, will be defined in parent via tank info
            FB : [0, 0.1, 1], //free board, will be defined in parent generally
            baffleT : [0, 0.0008, 2], //baffle thickness, will be defined in parent generally
            baffleS : [0.01, 0.1, 10], //baffle spacing, will be calculated in parent
            HL_bod : [0, 0.4, 1], //head loss, defined in parent
            washerT : [0.001, 0.003175, 0.2], //washer thickness
            washerOD : [0.125, 1, 3], //washer outer diameter
        //washer diameter

        },
        children : {
            "baffle" : {
                tree : baffleTree,
                inputs : {
                    T : "$.baffleT", //thickness
                    L : "$.bafflebottomL", //length
                    W : "$.channelW", //width, sheet width same as channel width
                    t : "corrugated", //type
                    mat : "PC", //material
                    ip : "$.ip", //implementation partner
                },
            },
        },
    };