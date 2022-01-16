using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Experimented with putting these in JSON for a little while, 
    but seeing as there probably won't be more than 20 of these
    I think it's fine to just hardcode it */
namespace Assets.WorldGen
{
    public class Beach : Biome
    {
        public Beach()
        {
            this.name = "beach";
            this.minHeight = 0;
            this.maxHeight = 0.2f;
            this.minMoisture = 0;
            this.maxMoisture = 1;
            this.color = new Color(179 / 255f, 162 / 255f, 133 / 255f);
            this.randomizationFactor = .2f;
            this.plantTypes = GetPlantInfo();
        }

        private List<PlantInfo> GetPlantInfo()
        {
            List<PlantInfo> plantTypes = new List<PlantInfo>();
            plantTypes.Add(new PlantInfo()
            {
                name = "Reeds",
                frequency = .7f,
                bunchChance = .8f,
                minBunchSize = 2,
                maxBunchSize = 5,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Rocks",
                frequency = .2f,
                bunchChance = .8f,
                minBunchSize = 2,
                maxBunchSize = 5,
                bunchRadius = 2
            });
            return plantTypes;
        }
    }

    public class Desert : Biome
    {
        public Desert()
        {
            this.name = "desert";
            this.minHeight = .2f;
            this.maxHeight = .5f;
            this.minMoisture = 0;
            this.maxMoisture = .4f;
            this.color = new Color(237 / 255f, 201 / 255f, 175 / 255f);
            this.randomizationFactor = .2f;
            this.plantTypes = GetPlantInfo();
        }

        private List<PlantInfo> GetPlantInfo()
        {
            // TODO: Idea: Moving Tumbleweeds
            List<PlantInfo> plantTypes = new List<PlantInfo>();
            plantTypes.Add(new PlantInfo()
            {
                name = "DeadBush",
                frequency = .3f,
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Rocks",
                frequency = .2f,
                bunchChance = 1f,
                minBunchSize = 2,
                maxBunchSize = 6,
                bunchRadius = 2
            });
            // plantTypes.Add(new PlantInfo() { name = "TreeLeafless", frequency = .2f });
            plantTypes.Add(new PlantInfo()
            {
                name = "TreeStump",
                frequency = .2f
            });
            return plantTypes;
        }
    }

    public class Deciduous : Biome
    {
        public Deciduous()
        {
            this.name = "deciduous";
            this.minHeight = .2f;
            this.maxHeight = .5f;
            this.minMoisture = .4f;
            this.maxMoisture = .6f;
            this.color = new Color(34 / 255f, 139 / 255f, 34 / 255f);
            this.randomizationFactor = .2f;
            this.plantTypes = GetPlantInfo();
        }

        private List<PlantInfo> GetPlantInfo()
        {
            List<PlantInfo> plantTypes = new List<PlantInfo>();
            plantTypes.Add(new PlantInfo()
            {
                name = "Bush",
                frequency = .6f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = .5f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "DeadBush",
                frequency = .2f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "FlowerBush",
                frequency = .3f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 8,
                bunchRadius = .5f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "FlowersTwoSided",
                frequency = .6f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Grass3D",
                frequency = 1,
                bunchChance = 1,
                minBunchSize = 2,
                maxBunchSize = 6,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Rocks",
                frequency = .1f,
                bunchChance = 1f,
                minBunchSize = 2,
                maxBunchSize = 6,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "TreeStump",
                frequency = .3f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "BirchTree",
                frequency = .7f
            });
            // plantTypes.Add(new PlantInfo() { name = "BirchTreeLeafless", frequency = .7f });
            plantTypes.Add(new PlantInfo()
            {
                name = "OakTree",
                frequency = .5f
            });
            return plantTypes;
        }
    }

    public class Wetlands : Biome
    {
        public Wetlands()
        {
            this.name = "wetlands";
            this.minHeight = .2f;
            this.maxHeight = .5f;
            this.minMoisture = .6f;
            this.maxMoisture = 1;
            this.color = new Color(109 / 255f, 115 / 255f, 95 / 255f);
            this.randomizationFactor = .2f;
            this.plantTypes = GetPlantInfo();
        }

        private List<PlantInfo> GetPlantInfo()
        {
            List<PlantInfo> plantTypes = new List<PlantInfo>();
            plantTypes.Add(new PlantInfo()
            {
                name = "Bush",
                frequency = .6f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "DeadBush",
                frequency = .6f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "FlowerBush",
                frequency = .6f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "FlowersTwoSided",
                frequency = .7f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Grass3D",
                frequency = 1,
                bunchChance = 1,
                minBunchSize = 2,
                maxBunchSize = 6,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Mushrooms",
                frequency = .8f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Reeds",
                frequency = .4f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Rocks",
                frequency = .2f,
                bunchChance = 1f,
                minBunchSize = 2,
                maxBunchSize = 6,
                bunchRadius = 2
            });
            // plantTypes.Add(new PlantInfo() { name = "TreeLeafless", frequency = .2f });
            plantTypes.Add(new PlantInfo()
            {
                name = "TreeStump",
                frequency = .2f
            });
            return plantTypes;
        }
    }

    public class Halloween : Biome
    {
        public Halloween()
        {
            this.name = "halloween";
            this.minHeight = .5f;
            this.maxHeight = .8f;
            this.minMoisture = 0;
            this.maxMoisture = .4f;
            this.color = new Color(128 / 255f, 0 / 255f, 128 / 255f);
            this.randomizationFactor = .2f;
            this.plantTypes = GetPlantInfo();
        }

        private List<PlantInfo> GetPlantInfo()
        {
            List<PlantInfo> plantTypes = new List<PlantInfo>();
            plantTypes.Add(new PlantInfo()
            {
                name = "DeadBush",
                frequency = .7f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Rocks",
                frequency = .1f,
                bunchChance = .4f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 1
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "TreeLeafless",
                frequency = .5f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "TreeStump",
                frequency = .4f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Grass3D",
                frequency = 1f,
                bunchChance = 1,
                minBunchSize = 2,
                maxBunchSize = 6,
                bunchRadius = 2
            });
            return plantTypes;
        }
    }

    public class Coniferous : Biome
    {
        public Coniferous()
        {
            this.name = "coniferous";
            this.minHeight = .5f;
            this.maxHeight = .8f;
            this.minMoisture = .4f;
            this.maxMoisture = .6f;
            this.color = new Color(84 / 255f, 96 / 255f, 79 / 255f);
            this.randomizationFactor = .2f;
            this.plantTypes = GetPlantInfo();
        }

        private List<PlantInfo> GetPlantInfo()
        {
            List<PlantInfo> plantTypes = new List<PlantInfo>();
            plantTypes.Add(new PlantInfo()
            {
                name = "DeadBush",
                frequency = .3f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Rocks",
                frequency = .3f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "TreeLeafless",
                frequency = .1f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "TreeStump",
                frequency = .3f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "PineTree",
                frequency = .7f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Grass3D",
                frequency = 1,
                bunchChance = 1,
                minBunchSize = 2,
                maxBunchSize = 6,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Bush",
                frequency = .6f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "FlowerBush",
                frequency = .6f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            return plantTypes;
        }
    }

    public class Tundra : Biome
    {
        public Tundra()
        {
            this.name = "tundra";
            this.minHeight = .5f;
            this.maxHeight = .8f;
            this.minMoisture = .6f;
            this.maxMoisture = 1;
            this.color = new Color(228 / 255f, 228 / 255f, 228 / 255f);
            this.randomizationFactor = .2f;
            this.plantTypes = GetPlantInfo();
        }

        private List<PlantInfo> GetPlantInfo()
        {
            List<PlantInfo> plantTypes = new List<PlantInfo>();
            plantTypes.Add(new PlantInfo()
            {
                name = "DeadBush",
                frequency = .2f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "Rocks",
                frequency = .3f,
                bunchChance = .7f,
                minBunchSize = 2,
                maxBunchSize = 3,
                bunchRadius = 2
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "TreeLeafless",
                frequency = .1f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "TreeStump",
                frequency = .3f
            });
            plantTypes.Add(new PlantInfo()
            {
                name = "PineTree",
                frequency = .8f
            });
            return plantTypes;
        }
    }

    public class Mountain : Biome
    {
        public Mountain()
        {
            this.name = "mountain";
            this.minHeight = .8f;
            this.maxHeight = 1;
            this.minMoisture = 0;
            this.maxMoisture = 1;
            this.color = new Color(30 / 255f, 30 / 255f, 30 / 255f);
            this.randomizationFactor = .2f;
            this.plantTypes = GetPlantInfo();
        }

        private List<PlantInfo> GetPlantInfo()
        {
            List<PlantInfo> plantTypes = new List<PlantInfo>();
            plantTypes.Add(new PlantInfo()
            {
                name = "Rocks",
                frequency = .2f,
                bunchChance = 1,
                minBunchSize = 2,
                maxBunchSize = 6,
                bunchRadius = 2
            });
            return plantTypes;
        }
    }
}