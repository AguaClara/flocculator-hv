FeatureScript 1521;
import(path : "onshape/std/geometry.fs", version : "1521.0");

import(path : "1802d3650943f2f88dc71465/989918c09b8ccc514fec7317/42a93e7d952620cd5e4b9afd", version : "e0ebb52a16bf3af96585f62c");
import(path : "6750b53736b16374e515f93d/71227b500f8b284979b992ba/181382047f743bb3a87d8136", version : "39b2ac8b0ef3cabc0f5dee7a");
import(path : "94569ca95d5169b5296f9bc5/672c60a12415d0d19238b12a/946213f130c9ca75ca65797e", version : "9fe6587da68cef149ed4447b");

/**
 * W is the required opening dimension of the gate
 **/
const tankChecks = {
            "W" : [0.01, 0.5, 2],
            "H" : [0.01, 0.5, 2],
            "T" : [0.003, 0.005, 0.02],
             "handleL" : [0, 0.2, 1],
            "ND" : [ 0.5, 1, 2],
            "SDR": [ 13.5, 26, 41],
            
        } as InputCheck;



/**
 * `function` which takes in `definition` and checks the minimum, maximum,
 * and default values from `tankChecks` before returning a `map`
 * @param definition: `map` of all variables required to construct a tank
 */
export const tankInstantiator = function(definition is map) returns map
    {
        return objectInstantiator(definition, tankChecks);
    };


/**
 * Hydraulic code and any calculations required before creating the geometry
 * as a standalone component
 * @param design: `map` of all variables required to construct a tank
 * @return `map` of designed tank with new entries like `L`, and 'AN'.
 **/
export const tankDesigner = function(design) returns map
    {
        design.uChannelT = round(0.0125 * design.W, 1 / 8 * inch);
        design.gapS = 0.1 * design.uChannelT;
        design.uChannelW = ceil(design.T + 2 * (design.gapS + design.uChannelT), 1 / 8 * inch);
        design.uChannelH = design.uChannelW;
        const gateInsertW = design.uChannelW - design.uChannelT - design.gapS;
        design.gateW = design.W + 2 * (gateInsertW);
        design.OW = 2 * (design.gapS + design.uChannelT) + design.gateW;
        design.gateH = design.H + gateInsertW;
        design.OH = (design.gapS + design.uChannelT) + design.gateH;
        design.pipe = pipeofD(design.ND, design.SDR, PipeSelectionType.ND);
        design.pipe.coupling = couplingofND(design.ND);
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
annotation { "Feature Type Name" : "tank" }
export const tankFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        treeInstantiatorFeature(context, tankInstantiator, tankDesigner);
    });
