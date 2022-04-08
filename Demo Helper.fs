FeatureScript 1605;
import(path : "onshape/std/geometry.fs", version : "1605.0");
import(path : "c81fce53dede81ef89860aa3/295ff2acebbd81bffc222cb4/b453944163f91ccf1477e3f0", version : "104bcbe80e3a372c576ae0b3");
import(path : "7a761c68d676ecc96febcf73/5624cb713c5f29310271a543/a35ab6b5b6eee3539b9b9de8", version : "c4aab1e77be075913fda14a2");
import(path : "1802d3650943f2f88dc71465/83125b65cf8a9d930d4d9ad1/42a93e7d952620cd5e4b9afd", version : "6467c2f0ae8ebe529adedab5");
import(path : "6750b53736b16374e515f93d/f4da4f3aa1b4adb72228ca1c/181382047f743bb3a87d8136", version : "e4647ddcdc54cbe2bb793255");

function opMakeDemo(context is Context, id is Id, definition is map)
{
     if (definition.configBooleans)
        {
            definition.printParams = definition.configprintParams;
            definition.plastic = definition.configplastic;
            definition.rep = definition.configrep;
        }

        definition = opMakeDemoGeometry(context, id, definition);
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
}


export const opMakeDemoGeometry = function(context is Context, id is Id, definition is map, demoPS) returns map
    {
        opMateConnector(context, id, { 'coordSystem' : coordSystem(vector(0 * meter, 0 * meter, 0 * meter), vector([1, 0, 0]), vector([0, 0, 1])) });
        const mateQ = qCreatedBy(id, EntityType.VERTEX);


        superDerive(context, id, {
                    "partStudio" : { buildFunction : demoPS::build, configuration : {} } as PartStudioData,
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