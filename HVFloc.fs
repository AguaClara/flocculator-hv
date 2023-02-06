FeatureScript 1793;
import(path : "5298e094b1acc40ce9092d44", version : "2264deb502b6c17aca31bb2c");
import(path : "2dbacff0d2cf5928d7043f72", version : "c5952c4353bd7ba3ae844b9c");
import(path : "c0af0d6b5703e7a8fb53f53f/a9de466febd1f9fd0dd6f78c/2b514867aec34e779649c734", version : "e6bb65db6855748a80d429fa");
import(path : "630baca1742eab8e31b42441/ae1f567054eb4c259ca7daec/828bc2e47f531cfe2ad5aebe", version : "f5ce374ceae4ecc3b8104c90");


/**
 * if floc+et=clarifier
 *
 **/


export const hvFlocTree = {
        name : "hvFloc",
        notes : {
            description : "horizontal vertical Flocculator",
            imagelink : "",
            textbooklink : "",
        },
        designers : {
            pre : hvFlocPreDesigner,
            post : hvFlocPostDesigner,
        // geometry : flocGeometry,
        },
        params : {
            done : false,
            ip : "GENERIC",
            rep : true,
            "Qm_max" : [1, 30, 500],
            "Q_pi" : [0, 1, 2],
            //"L_max" : [1, 7, 100],
            //"L_min" : [1, 7, 100], //use to force the same length as clarifier by increasing Gt
            "increaseGTtoMatchL" : false, // default to not increasing flocculator length
            "drainChannel" : false,
            "drainChannelH" : [0, 0.4, 1],
            "etL" : [0, 0, 10],
            "clarifierL" : [1, 7, 20],
            "humanChannelW_min" : [0.3, 0.5, 1],
            "baffleChannelW_max" : [0.5, 1.08, 2],
            "TEMP_min" : [0, 5, 40],
            "HL_bod" : [0, 0.4, 1],
            "minHS_pi" : [4, 6, 8],
            "maxHS_pi" : [8, 10, 12],
            "outletHW" : [0.3, 2, 5],
            "GT_min" : [0, 35000, 100000],
            "FB" : [0.05, 0.1, 0.5],
            "G_bod" : [20, 50, 130],
            "etWall" : true,
            "back" : true,
            "channelT" : [0, 0.15, 2],
            "baffleT_min" : [0, 0.0008, 0.5],
            "drainTI" : [600, 1200, 1800],
            "componentS" : [0.01, 0.05, 0.1],
        },
        execution : { order : ["tank", "baffleSet", "drain"] },
        children : {
            tank : {
                tree : tankTree,
                inputs :
                {
                    ip : "$.ip",
                    "H" : "$.H",
                    HW : "$.inletHW",
                    L : "$.L",
                    "W" : "$.channelW",
                    N : "$.channelN",
                    left : "$.etWall",
                    right : true,
                    front : true,
                    suppressFront : false,
                    back : "$.back",
                    bottom : "$.bottom",
                    top : false,
                    hasPort : true,
                    portH : "$.channelW",
                    portW : "$.baffle.S",
                    portSwap : false,
                },
            },
            baffleSet : {
                tree : baffleSetTree,
                inputs : {
                    rep : "$.rep",
                    ip : "$.ip",
                    channelN : "$.channelN",
                    channelT : "$.tank.wall.T",
                    channelW : "$.channelW",
                    channelL : "$.L",
                    tankH : "$.H",
                    FB : "$.FB",
                    baffleT : "$.baffle.T",
                    "baffleS" : "$.baffle.S",
                    HL_max : "$.HL_max",
                },
            },
            drain : {
                tree : elbowDrainPipeStubTree,
                inputs : {
                    rep : "$.rep",
                    ip : "$.ip",
                    FB : "$.FB",
                    "Qm_max" : "$.drainQm_max",
                    "HW" : "$.inletHW",
                    "ND_min" : 2,
                    "slabT" : "$.tank.slab.T",
                    "componentS" : "$.componentS",
                    "horizontalL" : "$.drainHorizontalL",
                },
            },
            sideDrain : {
                tree : elbowDrainPipeStubTree,
                inputs : {
                    rep : "$.rep",
                    ip : "$.ip",
                    FB : "$.FB",
                    "Qm_max" : "$.drainQm_max",
                    "HW" : "$.inletHW",
                    "ND_min" : 2,
                    "slabT" : "$.tank.slab.T",
                    "componentS" : "$.componentS",
                    "horizontalL" : "$.sideDrainHorizontalL",
                },
            },
        },
    };

/**
 * Custom check for VH Floc values, in the format "variable_name" : [min, default, max]
 * Values should be passed in unitless.
 * Units in parentheses are applied based on naming convention when deserializing
 *
 * Qm_max: maximum flow rate (L / second)
 * Q_pi: proportion of current flow to max (dimensionless)
 * TEMP_min: minimum temperature (Celsius)
 * HL_bod: maximum head loss (meter)
 * outletHW: maximum height of water at the outlet (meter)
 * GT_min: collision potential (dimensionless)
 * FB: freeboard (meter)
 * G_bod:  maximum velocity gradient allowed (second ^ -1) at lowest temperature
 * slabT: slab thickness (meter)
 * wallT: exterior wall thickness (meter)
 * channelWallT: interior wall thickness (meter)
 * drainTI: time required to drain the flocculator (second)
 */

export const hvFlocPreDesigner = function(design) returns map
    {
        design.NU = viscosityKinematic(design.TEMP_min);

        // Use the minimum of the velocity gradient set as the max for the sed tank to work and the value set by the max head loss.
        design.G = min((gravity * design.HL_bod / (design.NU * design.GT_min)), design.G_bod);
        design.TI = design.GT_min / design.G;
        if (design.drainChannel)
        {
            design.slab1T = queryCivilDim(design.ip, SheetType.SLAB1, SheetMaterial.AUTO, design.outletHW, ["factoryT"]).factoryT;
            design.outletHW = design.outletHW - design.drainChannelH - design.slab1T;
        }
        design.bottom = !design.drainChannel;

        if (design.etL > 0 * meter)
        {
            design.wallT = queryCivilDim(design.ip, SheetType.WALL, SheetMaterial.AUTO, design.outletHW, ["factoryT"]).factoryT; //ignores extra head loss https://aguaclara.github.io/Textbook/Flocculation/Floc_Derivations.html#equation-eq-gen-g-and-hl
            design.L = design.clarifierL - design.etL - design.wallT;
        }
        else
        {
            design.L = design.clarifierL;
        }

        design.VOL = design.Qm_max * design.TI;
        design.W_total = design.VOL / (design.L * design.outletHW);
        design.channelW_min = max(channelW_min(design), design.humanChannelW_min);

        if (design.W_total < design.channelW_min)
        {
            design.W_total = design.channelW_min;
            if (!design.increaseGTtoMatchL)
            {
                //design.L = design.VOL / (design.W_total * design.outletHW);
                design.outletHW_new = design.VOL / (design.W_total * design.L);
                design.drainChannelH += design.outletHW - design.outletHW_new;
                design.outletHW = design.outletHW_new;
            }
        }
        // need to make sure we don't specify a channel that is wider than the polycarbonate sheets
        design.channelN = max([floor(design.W_total / design.channelW_min), ceil(design.W_total / design.baffleChannelW_max)]); //make sure we don't try zero channels

        design.channelW = design.W_total / design.channelN;
        design = baffleS(design);

        design.baffle.T = querySheetDim(design.ip, SheetType.CORRUGATED, SheetMaterial.AUTO, design.baffleT_min, ["factoryT"]).factoryT;
        design.baffle.spacesN = floor(design.L / (design.baffle.S + design.baffle.T) / 2) * 2;

        // actual head loss given actual number of baffles
        design.HL_max = FlocHL(design);
        // actual inlet water level
        design.inletHW = design.outletHW + design.HL_max;
        design.H = design.inletHW + design.FB;

        design.portS = design.baffle.S;

        //actual collision potential
        design.GT = sqrt(gravity * design.HL_max * design.TI / design.NU);
        design.channelHW = ChannelHW(design);
        //each drain will cover at most two channels. The max flow is double the average
        design.drainQm_max = 2 * min(design.channelN, 2) * design.channelW * design.L * design.inletHW / design.drainTI;
        
        if (design.channelN % 2 == 1) //odd case so put a side drain at the beginning of the first channel
        {
            design.sideDrainN = 1;
        }
        else
        {
            design.sideDrainN = 0;
        }
        if (design.channelN == 1) // only one channel so don't add another drain at the downstream end of channel 1
        {
            design.drainN = 0;
        }
        else // put a drain at the downstream end of every odd channel 
        {
            design.drainN = ceil(design.channelN / 2);
        }
        
        design.wallT = queryCivilDim(design.ip, SheetType.WALL, SheetMaterial.AUTO, design.inletHW, ["factoryT"]).factoryT;
        design.drainHorizontalL = design.wallT + design.componentS;
        design.sideDrainHorizontalL = design.channelW / 2 + design.wallT + design.componentS;

        return design;
    };

export const hvFlocPostDesigner = function(design) returns map
    {
        design.OW = design.tank.OW;
        design.channelEven = floor(design.channelN / 2) == ceil(design.channelN / 2);
        design.drainWallB = design.baffleSet.baffle.pipe.hedgeB + design.baffleSet.baffle.pipe.colS / 2;
        return design;
    };

// export const flocGeometry = function(context is Context, id is Id, design is map)
//     {
//         const waterSketch = newSketchOnPlane(context, id + "sketch", {
//                     "sketchPlane" : XY_PLANE
//                 });

//         skLineSegment(waterSketch, "line1", {
//                     "start" : vector(0, 0) * inch,
//                     "end" : vector(1, 1) * inch
//                 });

//         skSolve(waterSketch);
//         debug(context, waterSketch, DebugColor.RED);
//         const myline = sketchEntityQuery(id + "sketch1", EntityType.EDGE, "line1");
//         debug(context, myline, DebugColor.RED);

//         //const waterLine = qSketchFilter(, waterSketch);

//         opExtrude(context, id + "extrude1", {
//                     "entities" : myline,
//                     "direction" : XY_PLANE.normal,
//                     "endBound" : BoundingType.BLIND,
//                     "endDepth" : design.baffle.S
//                 });


//         return design;
//     };


annotation { "Feature Type Name" : "HV Floc" }
export const hvFlocFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        treeInstantiatorFeature(context, id, hvFlocTree as InputTree);
    });

/**
 *
 *
 */

function baffleS(design)
{
    design.baffle = {};
    var err = 1.0;
    design.baffle.S = 0.01 * meter; //first guess
    var prevS = design.baffle.S;
    var count = 0;
    while ((err > 0.0001) && (count < 200))
    {
        count += 1;
        prevS = design.baffle.S;
        design.expN = ceil(design.outletHW / (design.baffle.S * design.maxHS_pi));
        design.baffle.expH = design.outletHW / design.expN; //distance between expansions
        design.HS_pi = design.baffle.expH / design.baffle.S;
        design.baffleK = Kbaffle(design.HS_pi, 3);
        design.baffle.S = (design.baffleK / (2 * design.baffle.expH * design.G ^ 2 * design.NU)) ^ (1 / 3) * design.Qm_max / design.channelW;
        err = abs((design.baffle.S - prevS) / (design.baffle.S + prevS));
    }
    //design.expN = ceil(design.outletHW / (design.baffle.S * design.maxHS_pi));
    //design.expN = 1;
    // design.baffle.expH = design.outletHW / design.expN; //distance between expansions
    //design.HS_pi = design.baffle.expH / design.baffle.S;
    //design.outletHW = design.baffle.S * design.maxHS_pi;
    //design.baffleK = Kbaffle(design.HS_pi, 3);
    design.V = design.Qm_max / (design.baffle.S * design.channelW);
    design.expHL = design.baffleK * design.V ^ 2 / (2 * gravity);
    return design;
}


function channelW_min(design is map)
{
    const a = (1 - baffleVC_pi) ^ 4 * design.minHS_pi;
    const b = 2 * (baffleVC_pi * ratioBaffleJetExpansion) ^ 2;
    return design.Qm_max / (design.NU * design.G ^ 2 * design.outletHW ^ 4) ^ (1 / 3) * (a / b) ^ (1 / 3);
}



function FlocHL(design is map)
{

    return design.expN * design.baffle.spacesN * design.channelN * design.expHL;
}

/**
 * This function calculates the number of flow expansions between water surfaces
 * Then it calculates the water surface elevations as a function of the current flow rate.
 * This is used to draw the water surface in each of the flocculator channels.
 */
function ChannelHW(design is map)
{
    var channelHW = makeArray(design.channelN);
    channelHW[0] = design.inletHW;
    for (var i = 1; i < design.channelN; i += 1)
    {
        channelHW[i] = channelHW[i - 1] - design.baffle.spacesN * design.expN * design.expHL;
    }
    return channelHW;

}


//IM - to fix
// - awk amount of end baffles
// - extra washers - will middle washers always go through both?
// - consider having thru pipes run through all baffles for small S
