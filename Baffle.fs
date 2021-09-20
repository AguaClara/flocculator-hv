FeatureScript 1576;
import(path : "onshape/std/geometry.fs", version : "1576.0");
import(path : "2fa81f50be25609bc956cd5f/9315fcf8489f0c0cc1a06a01/40a6bde79e4081741060af59", version : "24d9ce4bf05b3add5d64a574");

import(path : "c0af0d6b5703e7a8fb53f53f/ca3f04e79a55cdd9fedd1ad1/0f7fbd6e5e57d50d045b4ccd", version : "507858454a86a1de9513585e");

export const baffleTree =
{
        name : "baffle",
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
            lastchannel : false,
            tankH : [1, 2, 200], //height of tank... will be defined in parent via tank info
            channelW : [0.05, 0.5, 100], //width of channel, will be defined in parent via tank info
            channelL : [0, 7, 200], //length of channel, will be defined in parent via tank info
            FB : [0, 0.1, 1], //free board, will be defined in parent generally
            baffleT : [0, 0.0008, 2], //baffle thickness, will be defined in parent generally
            baffleS : [0.01, 0.1, 10], //baffle spacing, will be calculated in parent
            HL_bod : [0, 0.4, 1], //head loss, defined in parent
        },
        children : {
            "bottom" : {
                tree : sheetTree,
                inputs : {
                    T : "$.baffleT", //thickness
                    L : "$.bafflebottomL", //length
                    W : "$.channelW", //width, sheet width same as channel width
                    t : "corrugated", //type
                    mat : "PC", //material
                    ip : "$.ip", //implementation partner
                },
            },
            "top" : {
                tree : sheetTree,
                inputs : {
                    T : "$.baffleT", //thickness
                    L : "$.baffletopL", //length
                    W : "$.channelW", //width, sheet width same as channel width
                    t : "corrugated", //type
                    mat : "PC", //material
                    ip : "$.ip", //implementation partner
                },
            },
        },
    };

export const bafflePreDesigner = function(design) returns map
    {
        design.bafflebottomL = design.tankH - design.FB - design.HL_bod - design.baffleS; //length of bottom baffle
        design.baffletopL = design.tankH - design.FB / 2; //length of top baffle
        design.baffleN = floor(design.channelL / design.baffleS); //total number of baffles

        if (design.lastchannel == true)
        {
            design.baffleN = floor(design.baffleN / 2) * 2;
        }
        else
        {
            if ((floor(design.baffleN / 2) == ceil(design.baffleN / 2)) == true)
            {
                design.baffleN = design.baffleN - 1;
            }
        }

        design.bafflebottomN = ceil(design.baffleN / 2); //number of bottom baffles
        design.baffletopN = floor(design.baffleN / 2); //number of top baffles
        design.floorbottomS = 0*meter; //distance between bottom of baffle and tank bottom (bottom baffle)
        design.floortopS = design.baffleS; //distance between bottom of baffle and tank bottom (top baffle)


        return design;

    };

export const bafflePostDesigner = function(design) returns map
    {

        return design;

    };


annotation { "Feature Type Name" : "Baffle" }
export const baffleFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        treeInstantiatorFeature(context, id, baffleTree as InputTree);
    });
