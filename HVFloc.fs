FeatureScript 1483;
import(path : "onshape/std/geometry.fs", version : "1483.0");
import(path : "1802d3650943f2f88dc71465/989918c09b8ccc514fec7317/42a93e7d952620cd5e4b9afd", version : "e0ebb52a16bf3af96585f62c");
import(path : "6750b53736b16374e515f93d/71227b500f8b284979b992ba/181382047f743bb3a87d8136", version : "39b2ac8b0ef3cabc0f5dee7a");
import(path : "3859f0116fbf2e199237ee59/31a7f65b7abaefd99b5bab70/db7cf0a0f695f2c4e1854d8c", version : "e3df38a07643d8f3a7753fd7");
import(path : "4cc322f6b03a10200a5a6ffd/67e99e0e4769fd1af795c76a/57632593cd95821f373148c7", version : "2ca15dc2592687b811e11a87");
import(path : "94569ca95d5169b5296f9bc5/938ba4d703292d84de6901fb/946213f130c9ca75ca65797e", version : "04dab02e99faa2e0c4a5fb09");

// constants
const ratioPlaneJetExpansion = 0.116; //expansion ratio for plane jets
const baffleVC_pi = 0.6 ^ 2; // give a little factor of safety on head loss




const variablesToPassToChild = ["Qm_max", "TEMP_min", "FB", "wallT"];

/**
 * Custom check for VH Floc values, in the format "variable_name" : [min, default, max]
 * Values should be passed in unitless.
 * Units in parentheses are applied based on naming convention when deserializing
 *
 * Qm_max: maximum flow rate (L / second)
 * Q_pi: proportion of current flow to max (dimensionless)
 * TEMP_min: minimum temperature (Celsius)
 * HL_bod: maximum head loss (meter)
 * outletHW_max: maximum height of water at the outlet (meter)
 * GT_min: collision potential (dimensionless)
 * FB: freeboard (meter)
 * G_max: maximum velocity gradient (second ^ -1)
 * slabT: slab thickness (meter)
 * wallT: exterior wall thickness (meter)
 * channelWallT: interior wall thickness (meter)
 * drainTI: time required to drain the flocculator (second)
 */
const hvFlocChecks = {
            "Qm_max" : [0.1, 5, 30],
            "Q_pi" : [0, 1, 2],
            "L" : [1, 6, 20],
            "W_min" : [0.3, 0.45, 1],
            "TEMP_min" : [0, 15, 40],
            "HL_bod" : [0, 0.5, 1],
            //"K_min" : [2.6, 3.5, 5],
            "minHS_pi" : [ 3, 4, 5],
            "maxHS_pi" : [ 6, 8, 10],
            "outletHW_max" : [0, 2, 5],
            "GT_min" : [0, 35000, 100000],
            "FB" : [0.05, 0.1, 0.5],
            "G_max" : [1, 200, 50000],
            "slabT" : [0.001, 0.15, 0.5],
            "wallT" : [0.001, 0.15, 0.5],
            "channelWallT" : [0.001, 0.15, 0.5],
            "drain" : { instantiator : drainInstantiator, passVariables : ["wallT"] },
            "baffle" : { instantiator : baffleInstantiator, passVariables : variablesToPassToChild },
        } as InputCheck;

/**
 * `function` which takes in `definition` and checks the minimum, maximum,
 * and default values from `hvFlocChecks` before returning a `map`
 * @param definition: map of all variables required to construct a VH Flocculator
 */
export const hvFlocInstantiator = function(definition is map) returns map
    {
        return objectInstantiator(definition, hvFlocChecks);
    };

/**
 * Hydraulic code and any calculations required before creating the geometry
 * as a standalone component
 * @param design: `map` of all variables required to construct a VH Flocculator
 * @return `map` of designed VH Floc with new entries like `G`
 **/
export const hvFlocDesigner = function(design) returns map
    {

        design.NU = viscosityKinematic(design.TEMP_min);

        // Use the minimum of the velocity gradient set as the max for the sed tank to work and the value set by the max head loss.
        design.G = min((gravity * design.HL_bod / (design.NU * design.GT_min)), design.G_max);
        design.TI = design.GT_min / design.G;
        design.VOL = design.TI/design.Qm_max;
        design.W_total = design.VOL/(design.L * design.outletHW_max);
        println(design.W_total);
        design.channelW_min = channelW_min(design);
        println(design.channelW_min);
        design.channelN = floor(design.W_total/design.channelW_min);
        
        
        //rework everything below
        
        design.KE = baffleKE(design.minHS_pi);
        design.baffle.HE = OptimalHE(design);
        design.baffle.S = design.baffle.HE / design.minHS_pi;

        // Now find total length required for all of the back and forth flow
        design.baffle.TI = (design.baffle.HE * design.baffle.S ^ 2 / design.Qm_max);
        design.baffle.spacesN_est = round(design.TI / design.baffle.TI);

        // we want an odd  number of channels so that the water
        // enters the flocculator from the top and exits from the top.
        // We need to design an overflow to dump poorly flocculated water

        // find the maximum number of baffles spaces in each channel assuming that we need an even number in each channel
        design.baffle.spacesN_max = floor(design.outletHW_max / (design.baffle.S + design.baffle.T) / 2) * 2;
        design.channelN = ceil(design.baffle.spacesN_est / design.baffle.spacesN_max, 2);
        // Now calculate the required depth of the flocculator
        design.baffle.spacesN = ceilStep(design.baffle.spacesN_est / design.channelN, 2);

        design.outletHW = design.baffle.spacesN * (design.baffle.S + design.baffle.T) - design.baffle.T;
        // actual head loss given actual number of baffles
        design.HL_max = FlocHL(design);
        // actual inlet water level
        design.inletHW = design.outletHW + design.HL_max;
        // actual residence time in the active part of the flocculator (not including extra water due to head loss)
        design.TI = design.channelN * design.baffle.spacesN * design.baffle.TI;
        //actual collision potential
        design.GT = sqrt(gravity * design.HL_max * design.TI / design.NU);
        design.V = design.Qm_max / (design.baffle.S ^ 2);
        design.tankW = (design.baffle.S + design.channelWallT) * design.channelN - design.channelWallT;

        design.channelHW = ChannelHW(design);

        design.drain.S = design.baffle.S;
        design.drain.HE = design.baffle.HE;
        design.drain.HW = design.inletHW;
        design = treeDesigner(design, "drain", hvFlocChecks, drainDesigner);
        design.OW = design.baffle.S * design.channelN + design.channelWallT * (design.channelN - 1) + 2 * design.wallT;

        return design;
    };

/**
 * Design steps
 *    1) instantiate recursively to build the map (no units) that defines the design and print the map to the FeatureScript notices. This will be especially useful if the user needs the map to modify the design.
 *    2) apply units recursively
 *    3) design the feature including passing parameters to submaps that need to be set
 *    4) place the map with units in the context
 *    pass submaps thru superDerive in the part studio to bring other part studios into the design.
 **/
annotation { "Feature Type Name" : "HV Floc" }
export const hvFlocFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        treeInstantiatorFeature(context, hvFlocInstantiator, hvFlocDesigner);
    });

/**
 * TODO: Add comments to below code
 * const ratioPlaneJetExpansion = 0.116; //expansion ratio for plane jets
  * const baffleVC_pi = 0.6 ^ 2; // give a little factor of safety on head loss
*/

function baffleKE(HS_pi)
{
    const KE_min = (1 / baffleVC_pi - 1) ^ 2;
    const KE_unbounded_expansion = ((1 - baffleVC_pi) ^ 2 / (baffleVC_pi * ratioPlaneJetExpansion * HS_pi)) ^ 2;
    return max(KE_unbounded_expansion, KE_min);
}

function channelW_min(design is map)
{
     const a = (1 - baffleVC_pi)^4 * design.minHS_pi;
     const b = 2 *  (baffleVC_pi * ratioPlaneJetExpansion)^2;
     return design.Qm_max/(design.NU * design.G^2 *design.outletHW_max^4)^(1/3) * (a/b)^(1/3);
}


function OptimalHE(design is map)
{
    return (((design.minHS_pi ^ 2 * design.Qm_max) ^ 3 * design.KE / (2 * design.G ^ 2 * design.NU))) ^ (1 / 7);
}

function FlocHL(d is map)
{
    return d.baffle.spacesN * d.channelN * d.KE * d.Qm_max ^ 2 / (2 * gravity * d.baffle.S ^ 4);
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


