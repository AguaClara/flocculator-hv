FeatureScript 1521;
import(path : "onshape/std/geometry.fs", version : "1521.0");
import(path : "1802d3650943f2f88dc71465/989918c09b8ccc514fec7317/42a93e7d952620cd5e4b9afd", version : "e0ebb52a16bf3af96585f62c");
import(path : "6750b53736b16374e515f93d/71227b500f8b284979b992ba/181382047f743bb3a87d8136", version : "39b2ac8b0ef3cabc0f5dee7a");
import(path : "94569ca95d5169b5296f9bc5/672c60a12415d0d19238b12a/946213f130c9ca75ca65797e", version : "9fe6587da68cef149ed4447b");

/**
 * 
 **/
const tankChecks = {
              "L" : [1, 6, 20],
            "channelW" : [0.3, 0.45, 2],
            "outletHW" : [0, 2, 5],
            "inletHW" : [0, 2, 5],
            "channelN" : [1,2,20],
            "FB" : [0.05, 0.1, 0.5],
            "slabT" : [0.001, 0.15, 0.5],
            "wallT" : [0.001, 0.15, 0.5],
            "channelWallT" : [0.001, 0.15, 0.5],
            "portS" : [ 0.05, 0.15, 3]
            
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
        design.rotate = [];
        design.move = [];
        var direction = 1;
        for (var i = 0; i < design.channelN; i += 1)
        {
            if (direction == 0)
            {
                direction = 1;
            }
            else
            {
                direction = 0;
            }
            design.rotate = append(design.rotate, direction * 180*degree);
            design.move = append(design.move, i*(design.channelW + design.channelWallT) * (-1)^direction);
        }
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
