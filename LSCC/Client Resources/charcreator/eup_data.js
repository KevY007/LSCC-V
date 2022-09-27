/*
    Some explanation regarding the types.
    The EUP on RnC is linked with "types" so everything fits as good as it can.

    TORSO TYPES:
    Hands
    Elbows
    Shoulders

    SHIRT TYPES:
    JacketOpen
    ShortSleeves
    LongSleeves
    Armored

    UNDERSHIRT TYPES:
    InsideShirt
    DutyBelt

    PANT TYPES:
    Normal
    AnkleHigh

    BAGS:
    Radio
    Tie
    KevlarPiece

    SHOES:
    High
    Low

    ARMOR:
    KevlarArmor

    ACCESSORIES:
    HipHolster

    HATS:
    HatNormal,
    HatHeavy
*/

const Clothing = [
    {
        type: "JacketOpen",
        items: [468, 462, 455, 453, 444, 439, 438, 437, 434, 433, 431, 430, 428, 427, 426, 425, 424, 422]
    },
    {
        type: "ShortSleeves",
        items: [470, 469, 457, 448, 443, 420, 418, 417]
    },
    {
        type: "LongSleeves",
        items: [459, 458, 447, 442, 419]
    },
    {
        type: "Armored",
        items: [464, 463, 460, 451, 450]
    },
    {
        type: "InsideShirt",
        items: [242, 241, 232, 231, 229, 228, 223, 222, 208, 207]
    },
    {
        type: "DutyBelt",
        items: [227, 226, 225, 224, 219, 218, 217, 216, 212, 211, 210, 206, 205, 202, 201, 199, 198, 197, ]
    },
    {
        type: "KevlarUS",
        items: [214, 220]
    },
    {
        type: "Normal",
        items: [155, 152]
    },
    {
        type: "AnkleHigh",
        items: [163, 154, 153, 151]
    },
    {
        type: "Low",
        items: [110]
    },
    {
        type: "High",
        items: [109]
    },
    {
        type: "Tie",
        items: [115, 114]
    },
    {
        type: "HipHolster",
        items: [170, 171]
    },
    {
        type: "KevlarArmor",
        items: [63, 68, 70, 71, 77, 80, 82]
    },
    {
        type: "HeavyArmor",
        items: [62, 75]
    },
    {
        type: "Hands",
        items: [4, 22, 44]
    },
    {
        type: "Elbows",
        items: [0, 19, 41]
    },
    {
        type: "HatNormal",
        items: [46, 142]
    },
    {
        type: "HatHeavy",
        items: [141]
    },
]

const LinkedTypes = [
    {
        type: "JacketOpen",
        accepted:{
            Undershirts: ["InsideShirt"],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["HipHolster"],
            Armor: [], Bags: ["Tie"],
            Torso: ["Hands"], Hats: ["HatNormal"]
        }
    },
    {
        type: "ShortSleeves",
        accepted:{
            Undershirts: ["DutyBelt", "KevlarUS"],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["HipHolster"],
            Armor: ["KevlarArmor"], Bags: [],
            Torso: ["Elbows"], Hats: ["HatNormal"]
        }
    },
    {
        type: "LongSleeves",
        accepted:{
            Undershirts: ["DutyBelt", "KevlarUS"],
            Pants: ["Normal"],
            Shoes: ["Low"],
            Accessories: ["HipHolster"],
            Armor: ["KevlarArmor"], Bags: [],
            Torso: ["Hands"], Hats: ["HatNormal"]
        }
    },
    {
        type: "Armored",
        accepted:{
            Undershirts: ["DutyBelt"],
            Pants: ["AnkleHigh"],
            Shoes: ["High"],
            Accessories: ["HipHolster"],
            Armor: ["KevlarArmor", "HeavyArmor"], Bags: [],
            Torso: ["Hands"], Hats: ["HatHeavy"]
        }
    }
];