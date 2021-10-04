FeatureScript 1483;
import(path : "onshape/std/geometry.fs", version : "1483.0");
import(path : "2fa81f50be25609bc956cd5f/818b7dee375f1bc9bcc87be6/40a6bde79e4081741060af59", version : "f6009dd864bdb2390dd1205d");
import(path : "2dbacff0d2cf5928d7043f72", version : "89b56c20abc2afce0993350d");


//import(path : "3859f0116fbf2e199237ee59/3278c45cbd9cd1a648cbbdcf/db7cf0a0f695f2c4e1854d8c", version : "d43640e55104cc3a1d55eabe");
//import(path : "4cc322f6b03a10200a5a6ffd/67e99e0e4769fd1af795c76a/57632593cd95821f373148c7", version : "2ca15dc2592687b811e11a87");


// constants
const ratioPlaneJetExpansion = 0.116; //expansion ratio for plane jets
const baffleVC_pi = 0.6 ^ 2; // give a little factor of safety on head loss


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
            ip : "GENERIC",
            rep : false,
            "Qm_max" : [1, 30, 200],
            "Q_pi" : [0, 1, 2],
            "L" : [1, 7, 20],
            "humanChannelW_min" : [0.3, 0.45, 1],
            "baffleChannelW_max" : [1, 1.08, 2],
            "TEMP_min" : [0, 15, 40],
            "HL_bod" : [0, 0.4, 1],
            "minHS_pi" : [3, 4, 5],
            "maxHS_pi" : [6, 8, 10],
            "outletHW" : [0, 2, 5],
            "GT_min" : [0, 35000, 100000],
            "FB" : [0.05, 0.1, 0.5],
            "G_max" : [1, 200, 50000],
            "etWall" : false,
            "channelT" : [0, 0.15, 2],

        },
        execution : { order : ["tank", "baffleSet"] },
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
                    back : true,
                    bottom : true,
                    top : false,
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
                    channelT : "$.channelT", //not defined in this FS, just reference?? Also, check out the diff between interior and exterior wall thicknesses
                    channelW : "$.channelW",
                    channelL : "$.L",
                    tankH : "$.H",
                    FB : "$.FB",
                    //baffleT : "$.baffle.T",
                    "baffleS" : "$.baffle.S",
                    HL_bod : "$.HL_bod",
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
 * G_max: maximum velocity gradient (second ^ -1)
 * slabT: slab thickness (meter)
 * wallT: exterior wall thickness (meter)
 * channelWallT: interior wall thickness (meter)
 * drainTI: time required to drain the flocculator (second)
 */

export const hvFlocPreDesigner = function(design) returns map
    {

        design.NU = viscosityKinematic(design.TEMP_min);

        // Use the minimum of the velocity gradient set as the max for the sed tank to work and the value set by the max head loss.
        design.G = min((gravity * design.HL_bod / (design.NU * design.GT_min)), design.G_max);
        design.TI = design.GT_min / design.G;
        design.VOL = design.Qm_max * design.TI;
        design.W_total = design.VOL / (design.L * design.outletHW);
        design.channelW_min = max(channelW_min(design), design.humanChannelW_min);
        if (design.W_total < design.channelW_min)
        {
            design.W_total = design.channelW_min;
            design.L = design.VOL / (design.W_total * design.outletHW);
        }
        // need to make sure we don't specify a channel that is wider than the polycarbonate sheets
        design.channelN = max([floor(design.W_total / design.channelW_min), ceil(design.W_total / design.baffleChannelW_max)]); //make sure we don't try zero channels

        design.channelW = design.W_total / design.channelN;
        design = baffleS(design);
        //design.tankW = (design.channelW + design.channelWallT) * design.channelN - design.channelWallT;
        //rework everything below

        // find the maximum number of baffles spaces in each channel assuming that we need an even number in each channell
        // except the last channel where the inlet is low and the outlet is high and thus we need an odd number\
        // we will figure out the last channel by simply deleting the last baffle
        design.baffle.T = 0.003 * meter;
        design.baffle.spacesN = floor(design.L / (design.baffle.S + design.baffle.T) / 2) * 2;


        //design.outletHW = design.baffle.spacesN * (design.baffle.S + design.baffle.T) - design.baffle.T;
        // actual head loss given actual number of baffles
        design.HL_max = FlocHL(design);
        // actual inlet water level
        design.inletHW = design.outletHW + design.HL_max;
        design.H = design.inletHW + design.FB;
        // design.tank.inletHW = design.inletHW;
        // design.tank.channelW = design.channelW;
        // design.tank.channelN = design.channelN;
        design.portS = design.baffle.S;

        //actual collision potential
        design.GT = sqrt(gravity * design.HL_max * design.TI / design.NU);


        design.channelHW = ChannelHW(design);

        // design.drain.S = design.baffle.S;
        // design.drain.HE = design.baffle.HE;
        // design.drain.HW = design.inletHW;

        return design;
    };

export const hvFlocPostDesigner = function(design) returns map
    {
        design.OW = (design.channelW + design.tank.wall.T) * design.channelN;
        if (design.etWall)
        {
            design.OW = design.OW + design.tank.wall.T;
        }
        design.channelEven = floor(design.channelN / 2) == ceil(design.channelN / 2);
        
        // design.channelT = civilQ.T; //calling upon child to output civilQ.T
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
 * TODO: Add comments to below code
 * const ratioPlaneJetExpansion = 0.116; //expansion ratio for plane jets
 * const baffleVC_pi = 0.6 ^ 2; // give a little factor of safety on head loss
 */

function baffleS(design)
{
    design.baffle = {};
    //design.baffleK = baffleKE(design.maxHS_pi);
    var err = 1.0;

    // design = expH_min(design);
    // design.expN = max(floor(design.outletHW / design.baffle.expH_min), 1); // expansions per baffle
    // design.baffle.expH = design.outletHW / ceil(design.outletHW / (design.baffle.S * design.maxHS_pi)); //distance between expansions
    design.baffle.S = 0.01 * meter; //first guess

    //println("S is " ~ design.S);
    //println("H/S is" ~ design.baffle.expH / design.S);
    var prevS = design.baffle.S;
    var count = 0;
    while ((err > 0.0001) && (count < 200))
    {
        count += 1;
        prevS = design.baffle.S;
        design.expN = ceil(design.outletHW / (design.baffle.S * design.maxHS_pi));
        design.baffle.expH = design.outletHW / design.expN; //distance between expansions
        design.HS_pi = design.baffle.expH / design.baffle.S;
        design = baffleK(design);
        design.baffle.S = (design.baffleK / (2 * design.baffle.expH * design.G ^ 2 * design.NU)) ^ (1 / 3) * design.Qm_max / design.channelW;
        // println("S is " ~ design.baffle.S);
        // println("H/S is " ~ design.baffle.expH / design.baffle.S);
        // println("baffleK is " ~ design.baffleK);
        err = abs((design.baffle.S - prevS) / (design.baffle.S + prevS));
    }
    design.expN = ceil(design.outletHW / (design.baffle.S * design.maxHS_pi));
    design.baffle.expH = design.outletHW / design.expN; //distance between expansions
    design.HS_pi = design.baffle.expH / design.baffle.S;
    design = baffleK(design);
    design.V = design.Qm_max / (design.baffle.S * design.channelW);
    design.expHL = design.baffleK * design.V ^ 2 / (2 * gravity);
    return design;
}

// estimating the baffle loss coefficient using jet expansion rate and the vena contracta
function baffleK(design is map)
{
    if (design.HS_pi < design.minHS_pi)
    {
        design.HS_pi = design.minHS_pi;
    }
    const baffleK_min = (1 / baffleVC_pi - 1) ^ 2;
    const unboundedExpansionK = ((1 - baffleVC_pi) ^ 2 / (baffleVC_pi * ratioPlaneJetExpansion * design.HS_pi)) ^ 2;
    design.baffleK = max(unboundedExpansionK, baffleK_min);
    return design;
}

function channelW_min(design is map)
{
    const a = (1 - baffleVC_pi) ^ 4 * design.minHS_pi;
    const b = 2 * (baffleVC_pi * ratioPlaneJetExpansion) ^ 2;
    return design.Qm_max / (design.NU * design.G ^ 2 * design.outletHW ^ 4) ^ (1 / 3) * (a / b) ^ (1 / 3);
}


function expH_min(design is map)
{
    design.HS_pi = design.minHS_pi;
    design = baffleK(design);
    design.baffle.expH_min = (((design.minHS_pi ^ 2 * design.Qm_max) ^ 3 * design.baffleK / (2 * design.G ^ 2 * design.NU))) ^ (1 / 7);
    return design;
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
    // - channel length not matching with pipe length (why is it not perfectly aligned?)
    // - channel width is not aligning with baffles - pending (channelT needs adjustment)
    // - awk amount of end baffles
    // - extra washers - will middle washers always go through both?