FeatureScript 1589;
import(path : "onshape/std/geometry.fs", version : "1589.0");

import(path : "2fa81f50be25609bc956cd5f/9315fcf8489f0c0cc1a06a01/40a6bde79e4081741060af59", version : "24d9ce4bf05b3add5d64a574");
import(path : "ff0f26334dc9ed5e1dbbc027", version : "879f75f0267c65534457eac7");
baffleModule::import(path : "c2f4584cf9d8b1114f7ff5b4", version : "c5186f181b2af74ad2ba1b52");
import(path : "c81fce53dede81ef89860aa3/143d08de7750ac406af6ad04/b453944163f91ccf1477e3f0", version : "d3e1c56c5ddb796b94cd62ce");





export const baffleSetTree =
{
        name : "baffleSet",
        notes : {
            description : "",
            imagelink : "",
            textbooklink : "",
        },
        designers : {
            pre : baffleSetPreDesigner,
            //post : baffleSetPostDesigner,
            geometry : baffleSetGeometry,
        },
        params : {
            rep : true,
            ip : "app",
            channelN : [0, 1, 100], //number of baffle sets
            channelB : [0, 8, 200], //center to center distance between channels
            tankH : [1, 2, 200],
            channelW : [0.05, 0.5, 100],
            channelL : [0, 7, 200],
            FB : [0, 0.1, 1],
            baffleT : [0, 0.0008, 2],
            baffleS : [0.01, 0.1, 10],
            HL_bod : [0, 0.4, 1],
            washerT : [0.001, 0.003175, 0.2],
            washerOD : [0.0002, 1, 3],

        },
        children : {
            "baffle" : {
                tree : baffleTree,
                inputs : {

                    rep : "$.rep",
                    ip : "$.ip",
                    flowfront : "false", //diff
                    lastchannel : "false", //diff
                    tankH : "$.tankH",
                    channelW : "$.channelW",
                    channelL : "$.channelL",
                    FB : "$.FB",
                    baffleT : "$.baffleT",
                    baffleS : "$.baffleS",
                    HL_bod : "$.HL_bod",
                    washerT : "$.washerT",
                    washerOD : "$.washerOD",

                },
            },
        },
    };

export const baffleSetPreDesigner = function(design) returns map
    {



        return design;

    };

export const baffleSetPostDesigner = function(design) returns map
    {
        return design;

    };


export const baffleSetGeometry = function(context is Context, id is Id, design is map) returns map
    {
        for (var i = 1; i <= design.channelN; i += 1) //for every discrete number from 1 to N
        {


            if (i == design.channelN) //last channel?
            {
                design.lastchannel = true;
            }
            else
            {
                design.lastchannel = false;
            }


            if (floor(design.channelN / 2) == ceil(design.channelN / 2)) //if even, rotation of baffle
            {
                design.originY = 0; //location of origin
                design.originV1 = vector(0, 0, 1) * meter; //rotation
                design.originV2 = vector(1, 0, 0) * meter;
            }

            else
            {
                design.originY = -design.channelL / meter; //location of origin
                design.originV1 = vector(1, 0, 0) * meter; //rotation
                design.originV2 = vector(0, 0, 1) * meter;
            }

            design.originX = -design.channelB * (i - 1) / meter;
            //placement horizontally assuming placement at n=1 is 0

            var qlocation = coordSystem(vector(design.originX, design.originY, 0) * meter, design.originV1, design.originV2); //these are vectors, they do intersect at a certain point (rn it is the origin)

            opMateConnector(context, id + i, { 'coordSystem' : qlocation });
            const mateQ = qCreatedBy(id + i, EntityType.VERTEX);
            print(mateQ);
            superDerive(context, id + i, {
                        "partStudio" : { buildFunction : baffleModule::build, configuration : {} } as PartStudioData,
                        location : mateQ
                    });

        }


        // var qlocation = coordSystem(vector(0, 0, 0) * meter, vector(1, 0, 0) * meter, vector(0, 0, 1) * meter); //these are vectors, they do intersect at a certain point (rn it is the origin)

        // opMateConnector(context, id, { 'coordSystem' : qlocation });
        // const mateQ = qCreatedBy(id, EntityType.VERTEX);

        // superDerive(context, id, {
        //             "partStudio" : { buildFunction : baffleModule::build, configuration : {} } as PartStudioData,
        //             location : mateQ
        //         });
        return design;

    };

annotation { "Feature Type Name" : "Baffle Set" }
export const baffleSetFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        treeInstantiatorFeature(context, id, baffleSetTree as InputTree);
    });



//to do list
// - define params
// - define inputs
// - throw in superderive!
// - it will have to be iterative
// - if n = odd, flowfront = true (might not be relevant)
// - if n = N, lastchannel = true
// - could choose to rotate the entire baffle module and place at channelB
// keeps the beginning spacing at baffleS
// flowfront will always be true, that can be deleted from baffle

//questions
// -  if an input below is going to be defined in the next parent, does it still need to be defined in params?



// println(qlocation);
// // println(evaluateQuery(context, qlocation));
// design.location = qlocation;
// design.location = [line(vector(-1, -1, 0) * inch, vector(0, 0, -1)), line(vector(1, 1, 0) * inch, vector(0, 0, -1))];
