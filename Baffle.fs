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
            flowfront: false, //if water comes in at the bottom and moves from to back, hitting the baffles at the front
            lastchannel : false,
            tankH : [1, 2, 200], //height of tank... will be defined in parent via tank info
            channelW : [0.05, 0.5, 100], //width of channel, will be defined in parent via tank info
            channelL : [0, 7, 200], //length of channel, will be defined in parent via tank info
            FB : [0, 0.1, 1], //free board, will be defined in parent generally
            baffleT : [0, 0.0008, 2], //baffle thickness, will be defined in parent generally
            baffleS : [0.01, 0.1, 10], //baffle spacing, will be calculated in parent
            HL_bod : [0, 0.4, 1], //head loss, defined in parent
            washerT : [0.001, 0.003175, 0.2], //washer thickness
            washerOD : [0.125, 1, 3], //washer diameter

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
        design.baffleN = floor(design.channelL / design.baffleS) - 1; //total number of baffles

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

        design.bottom = {};
        design.top = {};

        design.bottom.N = ceil(design.baffleN / 2); //number of bottom baffles
        design.top.N = floor(design.baffleN / 2); //number of top baffles
        design.bottom.floorS = 0 * meter; //distance between bottom of baffle and tank bottom (bottom baffle)
        design.top.floorS = design.baffleS; //distance between bottom of baffle and tank bottom (top baffle)


        //holes - top & bottom
        design.pipe = {};
        design.pipe.ND = 0.5;
        
        const pipe = pipeofD(design.pipe.ND, 26, PipeSelectionType.ND); //to be updated
        design.pipe.ID = pipe.ID;
        design.pipe.OD = pipe.OD;
        design.pipe.L = design.channelL;
        
        design.pipe.colN = ceil(design.channelW / (0.25 * meter)); //random equation for number of pipe columns
        design.pipe.hedgeD = 0.1 * meter; //horizontal edge distance from middle of hole
        design.pipe.colS = (design.channelW - 2 * design.pipe.hedgeD) / (design.pipe.colN - 1); //pipe column spacing
        design.pipe.topvedgeD = 0.1 * meter; //vertical edge distance from middle of top hole
        design.pipe.botvedgeD = 0.15 * meter; //vertical edge distance from middle of bottom hole

        //holes - middle
        design.pipe.midrowN = 1; //number of middle spacer rows dependent on spacing & height
        design.pipe.rowS = (design.bafflebottomL - design.baffleS) / (design.pipe.midrowN + 1) + design.baffleS - design.pipe.botvedgeD; //spacing from midpoint of bottom pipe

        //washers
        design.washer = {};
        design.washer.ID = design.pipe.OD; 
        design.washerOD = design.washer.ID + design.washer.ID*0.5; //ADJUST
        
        //spacers
        design.spacer = {};
        
        design.spacer.ND = 0.75;
        
        const spacer = pipeofD(design.pipe.ND, 26, PipeSelectionType.ND); //to be updated
        design.spacer.ID = pipe.ID;
        design.spacer.OD = pipe.OD;
        design.spacer.L: design.baffleS;
        
        if (design.flowfront == true)
            { 
               design.washer.tobaffleS = design.washerT; //placement of washer against baffle
            }
        else
            {
                design.washer.tobaffleS = -design.baffleT;
            }
        
        
        
            
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





//to do list
//1. holes into baffle (based on sedimentation)
    //what is the edge distance? distance from top/bottom for top/bottom?
    //top/bottom holes a function of the width of the baffles
    //need variables for distance to edge and bottom/top, hole size (from 1/2")
    //what is the distance between holes?
    //how many rows of spacers? 0 or 1 for now, function of height and spacings
//2. washers into baffle (make out of sheets, remove/extrude)
    //only put upstream
//3. pipes that go through baffles
//4. spacer half pipes




// - is there a way to make the option of a superderive item not exist (for the case of N = 0?)
    //maybe a parameter that has the option of exists/not exists for sheet
    //this will be a pending question
