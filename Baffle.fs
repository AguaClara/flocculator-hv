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
            "plate" : { //check if this is right
                tree : sheetTree,
                inputs : {
                    T : "$.baffleT", //thickness
                    L : "$.baffleL", //length
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
        design.bafflebottomL = design.tankH - design.FB - design.HL_bod - design.baffleS;
        
        design.baffletopL = design.tankH - design.FB/2;
        
        design.baffleN = floor(design.channelL / design.baffleS); //total number of baffles = length of channel / s ; remember, has to be odd or even. should this happen

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

           return design;
   
    };

export const bafflePostDesigner = function(design) returns map
    {
        //design bottom baffles:
        //number of baffles: ceil(baffleN/2)
        //first baffle placed S distance from wall
        //repeat @ spacing of 2*baffleS
        //distance from floor: 0
        //length: tankH - FB - HL - s

        //design top baffles:
        //number of baffles: floor(baffleN/2)
        //first baffle placed 2*s distance from wall
        //repeat @ spacing of 2*baffleS
        //distance from floor: s
        //length: (tankH - FB/2) - s


        //SOME QUESTIONS:
        //how do we locate the baffles? I am assuming we will have to place them in relation to some sort of plane


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


//GOALS w/ baffle
//1. figure out if channel is last or not (DONE)
//2. decide on number of baffles based on that (how does calculated value of baffleS in HVfloc play into this? is the last spacing from baffle to wall allowed to be any spacing?) last=even, not last=odd (DONE!)
//3. design top baffles (length and og spots are different)
//4. design bottom baffles
