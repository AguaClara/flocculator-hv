FeatureScript 1605;
import(path : "onshape/std/geometry.fs", version : "1605.0");


demo::import(path : "16171bc5d51fe4caa0b06c4e", version : "e4bb7d6d1779857924c3d438");

export enum mateName
{
    sed
}

const demoPS = demo::build;

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
        annotation { "Name" : "sub map name", "Default" : "demo" }
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
       opMakeDemo(context, id, definition, demoPS);
    });



