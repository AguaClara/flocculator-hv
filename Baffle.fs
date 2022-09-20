FeatureScript 1793;
import(path : "c0af0d6b5703e7a8fb53f53f/a9de466febd1f9fd0dd6f78c/0f7fbd6e5e57d50d045b4ccd", version : "e4a662d5e31eb9ad319201eb");
import(path : "5298e094b1acc40ce9092d44", version : "2e15fbffbc89123716ab7ad2");



//import(path : "c0af0d6b5703e7a8fb53f53f/e37161fb429dff14c81fe0d3/0f7fbd6e5e57d50d045b4ccd", version : "9ed0e517508e0e029d8bc59c");

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
            tankH : [0.3, 2, 200], //height of tank, will be defined in parent via tank info
            channelW : [0.05, 0.5, 100], //width of channel, will be defined in parent via tank info
            channelL : [0, 7, 200], //length of channel, will be defined in parent via tank info
            FB : [0, 0.1, 1], //free board, will be defined in parent generally
            baffleT : [0, 0.0008, 1], //baffle thickness, will be defined in parent generally
            baffleS : [0.01, 0.1, 10], //baffle spacing, will be calculated in parent
            HL_max : [0, 0.4, 1], //head loss, defined in parent
            washerT : [0.001, 0.003175, 0.2],
        //washer thickness


        },
        children : {
            "bottom" : {
                tree : sheetTree,
                inputs : {
                    T_min : "$.baffleT", //thickness
                    L : "$.bafflebottomL", //length
                    W : "$.channelW", //width, sheet width same as channel width
                    sheetType : "CORRUGATED", //type
                    sheetMaterial : "POLYCARBONATE", //material
                    ip : "$.ip", //implementation partner
                },
            },
            "top" : {
                tree : sheetTree,
                inputs : {
                    T_min : "$.baffleT",
                    L : "$.baffletopL",
                    W : "$.channelW",
                    sheetType : "CORRUGATED",
                    sheetMaterial : "POLYCARBONATE",
                    ip : "$.ip",
                },
            },
            "washer" : {
                tree : sheetTree,
                inputs : {
                    T_min : "$.washerT",
                    L : "$.washerOD", //outer washer width
                    W : "$.washerOD", //outer washer width
                    sheetType : "SHEET",
                    sheetMaterial : "PVC",
                    ip : "$.ip",
                },
            },
        },
    };

export const bafflePreDesigner = function(design) returns map
    {
        design.ip = design.ip;
        //sheet
        design.bafflebottomL = design.tankH - design.FB - design.HL_max - design.baffleS; //length of bottom baffle
        design.baffletopL = design.tankH - (design.FB / 2) - design.baffleS; //length of top baffle
        design.baffleB = design.baffleS + design.baffleT; //center to center spacing between baffles
        design.baffleN = floor(design.channelL / design.baffleB) - 1; //total number of baffles

        if (design.lastchannel == true) //odd/even number of baffles depending on if last channel or not
        {
            design.baffleN = floor(design.baffleN / 2) * 2;
        }
        else
        {
            if (design.baffleN % 2 == 0) //if even
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
        design.pipe = queryPipeDim(0.5, design.ip, 26, ["ND", "ID", "OD", "schedule"]);

        design.fitting = queryFittingDim(design.pipe.ND, design.ip, FittingType.CAP, FittingShape.CAP, ConnectionStyle.SOCKET_CONNECT, ["OD", "socketL"]);

        design.fitting.R = design.fitting.OD / 2; //distance from node to outer cap, TBD
        design.pipe.L = design.channelL - 2 * design.fitting.R; //length of pipe (node to node)
        design.pipe.colN = ceil(design.channelW / (0.35 * meter)); //random equation for number of pipe columns, TBD
        design.pipe.hedgeB = 0.1 * meter; //horizontal edge distance from middle of hole


        if (design.pipe.colN == 1) //this is to ensure we aren't dividing something by zero for the colN=1 case
        {
            design.pipe.colS = 0 * meter;
        }
        else
        {
            design.pipe.colS = (design.channelW - 2 * design.pipe.hedgeB) / (design.pipe.colN - 1); //pipe column spacing
        }

        design.pipe.topvedgeB = 0.1 * meter; //vertical edge distance from middle of top hole
        design.pipe.botvedgeB = design.baffleS + 0.1 * meter; //vertical edge distance from middle of bottom hole

        //holes & pipe - middle
        design.pipe.midrowN = 1; //number of middle spacer rows dependent on spacing & height, TBD
        design.pipe.rowB = (design.bafflebottomL - design.baffleS) / (design.pipe.midrowN + 1) + design.baffleS - design.pipe.botvedgeB; //spacing from midpoint of bottom pipe

        //washers & spacers
        design.washer = {};
        design.washer.ID = design.pipe.OD;
        design.washerOD = design.washer.ID * 3; //TBD
        design.washer.tobaffleS = design.washerT; //spacing between washer & baffle

        design.spacer = queryPipeDim(design.pipe.OD, design.ip, 26, ["ND", "ID", "OD", "schedule"]); //these all have to be things that are in obtained by the query. HMMM... Maybe I should make a "return everything" option.

        design.spacer.lowerL = design.baffleS - design.washerT; //length of lower spacer
        design.spacer.upperL = design.baffleS + design.baffleB - design.washerT; //length of upper spacer
        design.spacer.lowerN = design.baffleN - 1; //number of lower spacers
        design.spacer.tobaffleS = design.washerT; //spacing between spacer & baffle


        design.spacer.topbackL = design.baffleS * 2 - design.fitting.socketL - design.fitting.R + design.baffleT; //length of top back spacers
        design.spacer.botbackL = design.baffleS - design.fitting.socketL - design.fitting.R; //length of bottom back spacers
        design.spacer.front1D = design.channelL - 2 * design.baffleB - design.fitting.socketL - design.fitting.R;
        design.spacer.botfront2D = design.baffleB * (design.baffleN - 2) + design.washerT;

        if (design.lastchannel == true) //length of top front spacers depending on if last channel or not
        {
            design.spacer.topfront2D = design.spacer.botfront2D;
        }
        else
        {

            design.spacer.topfront2D = design.baffleB * (design.baffleN - 3) + design.washerT;

        }


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
