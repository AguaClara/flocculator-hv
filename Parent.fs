FeatureScript 1576;
import(path : "onshape/std/geometry.fs", version : "1576.0");
import(path : "c2f4584cf9d8b1114f7ff5b4", version : "03c94a55893881da2d0e5612");
import(path : "16171bc5d51fe4caa0b06c4e", version : "a44b349706ec0cafb20082f4");



export const parentTree = {
        name : "parent",
        notes : {
            description : "",
            imagelink : "",
            textbooklink : "",
        },
        designers : {
            pre : parentPreDesigner, //edit
            post : parentPostDesigner, //edit
        },
        params : {
            rep : true,
            ip : "app",
            S : [0, 2, 10], //spacing between baffles
            T: [0, 0.1, 2], //thickness of baffle
        },
        execution : { order : ["hvFloc", "baffle"] },
        children : {
            square : {
                tree : hvFlocTree,
                inputs : {
                    ip : "$.ip",
                    rep : "$.rep",
                    Qm_max : "$.Qm_max",
                    Q_pi : "$.Q_pi",
                    L : "$.L",
                    humanChannelW_min : "$.humanChannelW_min",
                    baffleChannelW_max : "$.baffleChannelW_max",
                    TEMP_min : "$.TEMP_min",
                    HL_bod : "$.HL_bod",
                    minHS_pi : "$.minHS_pi",
                    maxHS_pi : "$.maxHS_pi",
                    outletHW : "$.outletHW",
                    GT_min : "$.GT_min",
                    FB : "$.FB",
                    G_max : "$.G_max",
                    etWall : "$.etWall",
                }
            },
            baffle : {
                tree : baffleTree,
                inputs : {
                    rep : "$.rep",
                    ip : "$.ip",
                    tankH : [1, 2, 200], //height of tank... will be defined in parent via HVFloc
                    channelW : "$.channelW",
                    channelL : "$.L",
                    FB : "$.FB", 
                    baffleT : "$.T", 
                    baffleS : "$.S", 
                },
            },
        },
    };
