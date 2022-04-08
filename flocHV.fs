FeatureScript 1605;
import(path : "onshape/std/geometry.fs", version : "1605.0");
import(path : "7a761c68d676ecc96febcf73/5624cb713c5f29310271a543/a35ab6b5b6eee3539b9b9de8", version : "c4aab1e77be075913fda14a2");
import(path : "c81fce53dede81ef89860aa3/295ff2acebbd81bffc222cb4/b453944163f91ccf1477e3f0", version : "104bcbe80e3a372c576ae0b3");
import(path : "1802d3650943f2f88dc71465/83125b65cf8a9d930d4d9ad1/42a93e7d952620cd5e4b9afd", version : "6467c2f0ae8ebe529adedab5");
import(path : "6750b53736b16374e515f93d/f4da4f3aa1b4adb72228ca1c/181382047f743bb3a87d8136", version : "e4647ddcdc54cbe2bb793255");
flocHV::import(path : "16171bc5d51fe4caa0b06c4e", version : "9ef2faaeea3533a59a32819c");

export enum mateName
{
    sed
}


export type DemoParams typecheck canBeDemoParams;

export predicate canBeDemoParams(definition)
{

    annotation { "Name" : "Use configuration for booleans", "Default" : false }
    definition.configBooleans is boolean;

    if (definition.configBooleans)
    {
        annotation { "Name" : "Print map of inputs in FeatureScript notices", "Default" : "false" }
        isAnything(definition.configprintParams);

        annotation { "Name" : "Show internal components", "Default" : "true" }
        isAnything(definition.configplastic);

        annotation { "Name" : "Replicate all components", "Default" : "true" }
        isAnything(definition.configrep);
    }
    else
    {
        annotation { "Name" : "Print map of inputs in FeatureScript notices", "Default" : false }
        definition.printParams is boolean;

        annotation { "Name" : "Show internal components", "Default" : true }
        definition.plastic is boolean;

        annotation { "Name" : "Replicate all components", "Default" : true }
        definition.rep is boolean;
    }


    annotation { "Name" : "Location(s)", "Filter" : BodyType.MATE_CONNECTOR, "MaxNumberOfPicks" : 1 }
    definition.location is Query;

    annotation { "Name" : "From Mate Connector Name" }
    definition.mcName is mateName;

    annotation { "Name" : "Place design variables in context", "Default" : true }
    definition.mapToContext is boolean;

    if (definition.mapToContext)
    {
        annotation { "Name" : "sub map name", "Default" : "flocHV" }
        definition.submap is string;
    }
}

export type FlocHVParams typecheck canBeFlocHVParams;

export predicate canBeFlocHVParams(definition)
{
    annotation { "Name" : "Flow rate (L/s)", "Default" : "30" }
    isAnything(definition.Qm_max);

    annotation { "Name" : "Minimum Temperature (C)" }
    isReal(definition.TEMP_min, { (unitless) : [0, 5, 20] } as RealBoundSpec);

    annotation { "Name" : "Channel length" }
    isLength(definition.L, { (meter) : [3, 7, 40] } as LengthBoundSpec);

    annotation { "Name" : "Outlet water depth" }
    isLength(definition.outletHW, { (meter) : [1, 2, 5] } as LengthBoundSpec);

    annotation { "Name" : "Collision Potential (Gt)" }
    isReal(definition.GT_min, { (unitless) : [20000, 35000, 100000] } as RealBoundSpec);

    annotation { "Name" : "Velocity Gradient (1/s)" }
    isReal(definition.G_bod, { (unitless) : [20, 50, 80] } as RealBoundSpec);
}

annotation { "Feature Type Name" : "Flocculator HV" }
export const flocculatorHV = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        canBeFlocHVParams(definition);
        canBeDemoParams(definition);
    }
    {
        if (definition.configBooleans)
        {
            definition.printParams = definition.configprintParams;
            definition.plastic = definition.configplastic;
            definition.rep = definition.configrep;
        }

        definition = opMakeflocHVGeometry(context, id, definition);
        if (definition.mapToContext)
        {
            definition[definition.submap].mapToContext = undefined;
            definition[definition.submap].location = undefined;
            definition[definition.submap].submap = undefined;
            definition[definition.submap].done = undefined;
        }
        else
        {
            definition[definition.submap] = {};
        }
        moveAndMap(context, id, definition, definition.qPartBody, definition[definition.submap]);
    });



export const opMakeflocHVGeometry = function(context is Context, id is Id, definition is map) returns map
    {
        opMateConnector(context, id, { 'coordSystem' : coordSystem(vector(0 * meter, 0 * meter, 0 * meter), vector([1, 0, 0]), vector([0, 0, 1])) });
        const mateQ = qCreatedBy(id, EntityType.VERTEX);


        superDerive(context, id, {
                    "partStudio" : { buildFunction : flocHV::build, configuration : {} } as PartStudioData,
                    "location" : mateQ, //has to be query
                    "useOverrides" : true,
                    "customConfiguration" : true,
                    "configurationString" : mapToJSON(serializer(definition)),
                    "configEntries" : [{ "keyList" : ["plastic"], "value" : definition.plastic }],
                    "includeVariables" : definition.mapToContext,
                    "includeVariablesMethod" : IMPORT_VARIABLES_TYPE.INNER_VARIABLE,
                    "variableName" : definition.submap,
                    "mcName" : toString(definition.mcName),
                });
        if (definition.mapToContext)
        {
            definition[definition.submap] = getVariable(context, definition.submap);
        }

        definition.qPartBody = qCreatedBy(id, EntityType.BODY);
        return definition;
    };
