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
            ip : "GENERIC",
            lastchannel : false,
            tankH : [1, 2, 200], //height of tank, will be defined in parent via tank info
            channelW : [0.05, 0.5, 100], //width of channel, will be defined in parent via tank info
            channelL : [0, 7, 200], //length of channel, will be defined in parent via tank info
            FB : [0, 0.1, 1], //free board, will be defined in parent generally
            baffleT : [0, 0.0008, 2], //baffle thickness, will be defined in parent generally
            baffleS : [0.01, 0.1, 10], //baffle spacing, will be calculated in parent
            HL_bod : [0, 0.4, 1], //head loss, defined in parent
            washerT : [0.001, 0.003175, 0.2], //washer thickness


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
                    T : "$.baffleT",
                    L : "$.baffletopL",
                    W : "$.channelW",
                    t : "corrugated",
                    mat : "PC",
                    ip : "$.ip",
                },
            },
            "washer" : {
                tree : sheetTree,
                inputs : {
                    T : "$.washerT", //thickness
                    L : "$.washerOD", //outer washer width
                    W : "$.washerOD", //outer washer width
                    t : "sheet", //type
                    mat : "PVC", //material
                    ip : "$.ip", //implementation partner
                },
            },
        },
    };

export const bafflePreDesigner = function(design) returns map
    {

        //sheet
        design.bafflebottomL = design.tankH - design.FB - design.HL_bod - design.baffleS; //length of bottom baffle
        design.baffletopL = design.tankH - design.FB / 2; //length of top baffle
        design.baffleB = design.baffleS + design.baffleT; //center to center spacing between baffles
        design.baffleN = floor(design.channelL / design.baffleB) - 1; //total number of baffles

        if (design.lastchannel == true) //odd/even number of baffles depending on if last channel or not
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

        design.bottom = {};
        design.top = {};

        design.bottom.N = ceil(design.baffleN / 2); //number of bottom baffles
        design.top.N = floor(design.baffleN / 2); //number of top baffles
        design.bottom.floorS = 0 * meter; //distance between bottom of baffle and top of tank floor (bottom baffle)
        design.top.floorS = design.baffleS; //distance between bottom of baffle and top of tank floor (top baffle)


        //holes & pipe - top & bottom
        design.pipe = {};
        design.pipe.ND = 0.5; //nominal diameter
        design.pipe.SDR = 26; //standard diameter ratio

        const pipe = pipeofD(design.pipe.ND, design.pipe.SDR, PipeSelectionType.ND); //to be updated
        design.pipe.ID = pipe.ID; //inner diameter
        design.pipe.OD = pipe.OD; //outer diameter
        design.pipe.L = design.channelL; //length of pipe

        design.pipe.colN = ceil(design.channelW / (0.25 * meter)); //random equation for number of pipe columns, TBD
        design.pipe.hedgeB = 0.1 * meter; //horizontal edge distance from middle of hole
        design.pipe.colS = (design.channelW - 2 * design.pipe.hedgeB) / (design.pipe.colN - 1); //pipe column spacing
        design.pipe.topvedgeB = 0.1 * meter; //vertical edge distance from middle of top hole
        design.pipe.botvedgeB = 0.15 * meter; //vertical edge distance from middle of bottom hole

        //holes & pipe - middle
        design.pipe.midrowN = 1; //number of middle spacer rows dependent on spacing & height, TBD
        design.pipe.rowB = (design.bafflebottomL - design.baffleS) / (design.pipe.midrowN + 1) + design.baffleS - design.pipe.botvedgeB; //spacing from midpoint of bottom pipe

        //washers & spacers
        design.washer = {};
        design.washer.ID = design.pipe.OD;
        design.washerOD = design.washer.ID*3; //TBD
        design.washer.tobaffleS = design.washerT; //spacing between washer & baffle

        design.spacer = {};

        design.spacer.ND = 0.75;
        design.spacer.SDR = 26;

        const spacer = pipeofD(design.spacer.ND, design.spacer.SDR, PipeSelectionType.ND); //TBD
        design.spacer.ID = spacer.ID;
        
        design.spacer.lowerL = design.baffleS - design.washerT; //length of lower spacer
        design.spacer.upperL = design.baffleS + design.baffleB - design.washerT; //length of upper spacer
        design.spacer.lowerN = design.baffleN - 1; //number of lower spacers
        design.spacer.tobaffleS = design.washerT; //spacing between spacer & baffle
        
        design.fittingL = 0.05*meter; //length of fitting, TBD
        design.spacer.topbackL = design.baffleS*2 - design.fittingL; //length of top back spacers
        design.spacer.bottombackL = design.baffleS - design.fittingL; //length of bottom back spacers
        //design.spacer.frontL = design.channelL - design.spacer.backL - design.baffleT - design.baffleB*(design.baffleN - 1) - design.fittingL; //length of front spacers
        //design.spacer.bottomfrontL = design.channelL - design.spacer.backL - design.baffleT - design.baffleB*(design.baffleN - 1) - design.fittingL; //length of front spacers



        return design;

    };

export const bafflePostDesigner = function(design) returns map
    {

        design.spacer.upperN = design.top.N - 1; //number of upper spacers
        design.pipe.toptobotB = design.top.L - design.pipe.topvedgeB - design.pipe.botvedgeB + design.baffleS; //distance from top to bottom pipe/hole/washers
        
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






//to do list
// - half pipes
// - bring superderive into code so that it can sometimes not exist depending on the length of the tank (N = O case)

