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
    Tie

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

// female shirt 405-439 crashes game
// female shoes 90-111 crash the game
// female bags 89-110 crash the game
// bags 137 female = kevlar
const FemaleClothing = [
    {
        clothing: "Shirt",
        type: "JacketOpen",
        items: [467, 466, 464, 463, 462, 461, 458, 456, 454, 453, 447]
    },
    {
        clothing: "Shirt",
        type: "ShortSleeves",
        items: [465, 455, 444, 442, 441]
    },
    {
        clothing: "Shirt",
        type: "LongSleeves",
        items: [470, 460, 443]
    },
    {
        clothing: "Shirt",
        type: "Armored",
        items: [450]
    },
    {
        clothing: "Undershirt",
        type: "InsideShirt",
        items: [242]
    },
    {
        clothing: "Undershirt",
        type: "DutyBelt",
        items: [243, 241, 238, 237, 236]
    },
    {
        clothing: "Undershirt",
        type: "KevlarUS",
        items: []
    },
    {
        clothing: "Pants",
        type: "Normal",
        items: [163, 161]
    },
    {
        clothing: "Pants",
        type: "AnkleHigh",
        items: [162, 160, 159, 158]
    },
    {
        clothing: "Shoes",
        type: "Low",
        items: [27]
    },
    {
        clothing: "Shoes",
        type: "High",
        items: [25]
    },
    {
        clothing: "Accessory 1",
        type: "Tie",
        items: [115, 114]
    },
    {
        clothing: "Accessory 2",
        type: "HipHolster",
        items: [127, 126]
    },
    {
        clothing: "Armor",
        type: "KevlarArmor",
        items: [58, 59, 62, 66, 69, 76]
    },
    {
        clothing: "Armor",
        type: "HeavyArmor",
        items: [57, 61, 70, 74, 81]
    },
    {
        clothing: "Handwear",
        type: "Hands",
        items: [3, 26, 36]
    },
    {
        clothing: "Handwear",
        type: "Elbows",
        items: [0, 20, 35]
    },
    {
        clothing: "Hats",
        type: "HatNormal",
        items: [45, 141]
    },
    {
        clothing: "Hats",
        type: "HatHeavy",
        items: [140]
    },
]

const MaleClothing = [
    {
        clothing: "Shirt",
        type: "JacketOpen",
        items: [468, 462, 455, 453, 444, 439, 438, 437, 434, 433, 431, 430, 428, 427, 426, 425, 424, 422]
    },
    {
        clothing: "Shirt",
        type: "ShortSleeves",
        items: [470, 469, 457, 448, 443, 420, 418, 417]
    },
    {
        clothing: "Shirt",
        type: "LongSleeves",
        items: [459, 458, 447, 442, 419]
    },
    {
        clothing: "Shirt",
        type: "Armored",
        items: [464, 463, 460, 451, 450]
    },
    {
        clothing: "Undershirt",
        type: "InsideShirt",
        items: [242, 241, 232, 231, 229, 228, 223, 222, 208, 207]
    },
    {
        clothing: "Undershirt",
        type: "DutyBelt",
        items: [227, 226, 225, 224, 219, 218, 217, 216, 212, 211, 210, 206, 205, 202, 201, 199, 198, 197]
    },
    {
        clothing: "Undershirt",
        type: "KevlarUS",
        items: [-1, 214, 220]
    },
    {
        clothing: "Pants",
        type: "Normal",
        items: [155, 152]
    },
    {
        clothing: "Pants",
        type: "AnkleHigh",
        items: [163, 154, 153, 151]
    },
    {
        clothing: "Shoes",
        type: "Low",
        items: [110]
    },
    {
        clothing: "Shoes",
        type: "High",
        items: [109]
    },
    {
        clothing: "Accessory 1",
        type: "Tie",
        items: [-1, 115, 114]
    },
    {
        clothing: "Accessory 2",
        type: "HipHolster",
        items: [-1, 170, 171]
    },
    {
        clothing: "Armor",
        type: "KevlarArmor",
        items: [-1, 63, 68, 70, 71, 77, 80, 82]
    },
    {
        clothing: "Armor",
        type: "HeavyArmor",
        items: [-1, 62, 75]
    },
    {
        clothing: "Handwear",
        type: "Hands",
        items: [4, 22, 44]
    },
    {
        clothing: "Handwear",
        type: "Elbows",
        items: [0, 19, 41]
    },
    {
        clothing: "Hats",
        type: "HatNormal",
        items: [-1, 46, 142]
    },
    {
        clothing: "Hats",
        type: "HatHeavy",
        items: [-1, 141]
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