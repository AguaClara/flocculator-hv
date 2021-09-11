FeatureScript 1483;
import(path : "onshape/std/geometry.fs", version : "1483.0");
import(path : "2fa81f50be25609bc956cd5f/9315fcf8489f0c0cc1a06a01/40a6bde79e4081741060af59", version : "24d9ce4bf05b3add5d64a574");

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
        },
        params : {
            ip : "GENERIC",
            rep : true,
            "Qm_max" : [5, 30, 200],
            "Q_pi" : [0, 1, 2],
            "L" : [1, 6, 20],
            "humanChannelW_min" : [0.3, 0.45, 1],
            "baffleChannelW_max" : [1, 1.08, 2],
            "TEMP_min" : [0, 15, 40],
            "HL_bod" : [0, 0.5, 1],
            "minHS_pi" : [3, 4, 5],
            "maxHS_pi" : [6, 8, 10],
            "outletHW" : [0, 2, 5],
            "GT_min" : [0, 35000, 100000],
            "FB" : [0.05, 0.1, 0.5],
            "G_max" : [1, 200, 50000],
        },
        execution : { order : ["tank"] },
        children : {
            tank : {
                tree : tankTree,
                inputs :
                {
                    ip : "$.ip",
                    "FB" : "$.FB",
                    HW : "$.inletHW",
                    L : "$.L",
                    "W" : "$.channelW",
                    N : "$.channelN",
                    left : true,
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
        // need to make sure we don't specify a channel that is wider than the polycarbonate sheets
        design.channelN = max([floor(design.W_total / design.channelW_min), ceil(design.W_total / design.baffleChannelW_max)]); //make sure we don't try zero channels
        // design.multipleChannel = (design.channelN > 1);
        // design.morethan2Channels = (design.channelN > 2);
        design.channelW = design.W_total / design.channelN;
        design.KE = baffleKE(design.maxHS_pi);

        design = baffleS(design);
        design.HS_pi = design.baffle.expH / design.baffle.S;
        //design.tankW = (design.channelW + design.channelWallT) * design.channelN - design.channelWallT;
        //rework everything below

        // find the maximum number of baffles spaces in each channel assuming that we need an even number in each channell
        // except the last channel where the inlet is low and the outlet is high and thus we need an odd number\
        // we will figure out the last channel by simply deleting the last baffle
        design.baffle.T = 0.003 * meter;
        design.baffle.spacesN = floor(design.L / (design.baffle.S + design.baffle.T) / 2) * 2;
        design.V = design.Qm_max / (design.baffle.S * design.channelW);

        //design.outletHW = design.baffle.spacesN * (design.baffle.S + design.baffle.T) - design.baffle.T;
        // actual head loss given actual number of baffles
        design.HL_max = FlocHL(design);
        // actual inlet water level
        design.inletHW = design.outletHW + design.HL_max;

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
        // design.OW = design.baffle.S * design.channelN + design.channelWallT * (design.channelN - 1) + 2 * design.wallT;
        return design;
    };

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
    //design.KE = baffleKE(design.maxHS_pi);
    var err = 1.0;
    design.baffle.expH_max = OptimalHE(design);
    //design.S = (design.KE / (2 * design.baffle.expH * design.G ^ 2 * design.NU)) ^ (1 / 3) * design.Qm_max / design.channelW; //first guess
    //println("S is " ~ design.S);
    //println("H/S is" ~ design.baffle.expH / design.S);
    var prevS = 0 * meter;
    var count = 0;
    while ((err > 0.0001) && (count < 200))
    {
        count += 1;
        prevS = design.S;

        
        design.expN = ceil(design.outletHW / design.baffle.expH_max); // expansions per baffle
        design.baffle.expH = design.outletHW / design.expN; //distance between expansions
        design.KE = baffleKE(design.baffle.expH / prevS);
        design.S = (design.KE / (2 * design.baffle.expH * design.G ^ 2 * design.NU)) ^ (1 / 3) * design.Qm_max / design.channelW;
        println("S is " ~ design.S);
        println("H/S is" ~ design.baffle.expH / design.S);
        err = abs((design.S - prevS) / (design.S + prevS));
    }
    return design;
}

// estimating the baffle loss coefficient using jet expansion rate and the vena contracta
function baffleKE(HS_pi)
{
    const KE_min = (1 / baffleVC_pi - 1) ^ 2;
    const KE_unbounded_expansion = ((1 - baffleVC_pi) ^ 2 / (baffleVC_pi * ratioPlaneJetExpansion * HS_pi)) ^ 2;
    return max(KE_unbounded_expansion, KE_min);
}

function channelW_min(design is map)
{
    const a = (1 - baffleVC_pi) ^ 4 * design.minHS_pi;
    const b = 2 * (baffleVC_pi * ratioPlaneJetExpansion) ^ 2;
    return design.Qm_max / (design.NU * design.G ^ 2 * design.outletHW ^ 4) ^ (1 / 3) * (a / b) ^ (1 / 3);
}


function OptimalHE(design is map)
{
    return (((design.minHS_pi ^ 2 * design.Qm_max) ^ 3 * design.KE / (2 * design.G ^ 2 * design.NU))) ^ (1 / 7);
}

function FlocHL(d is map)
{

    return d.expN * d.baffle.spacesN * d.channelN * d.KE * d.V ^ 2 / (2 * gravity);
}

/**
 * This function calculates the number of flow expansions between water surfaces
 * Then it calculates the water surface elevations as a function of the current flow rate.
 * This is used to draw the water surface in each of the flocculator channels.
 */
function ChannelHW(d is map)
{
    const baffleHL = d.KE * (d.Q_pi * d.Qm_max) ^ 2 / (2 * gravity * d.baffle.S ^ 4);
    var channelHW = makeArray(d.channelN);
    var baffleSpacesN = 0;
    var myeven = true;
    channelHW[0] = d.outletHW + baffleHL * d.baffle.spacesN * d.channelN;
    for (var i = 1; i < d.channelN; i += 1)
    {
        myeven = !myeven;
        if (myeven)
        {
            baffleSpacesN = 1; //one bend as water flows through channel wall
        }
        else
        {
            baffleSpacesN = 2 * d.baffle.spacesN - 1;
        }

        channelHW[i] = channelHW[i - 1] - baffleSpacesN * baffleHL;
    }
    return channelHW;

}


