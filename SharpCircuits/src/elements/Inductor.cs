namespace SharpCircuit.src.elements
{
    public class Inductor : PassiveElement
    {
        public override double Value
        {
            get => inductance;
            set => inductance = value;
        }
        /// <summary>
        /// Inductance (H)
        /// </summary>
        public double inductance { get; set; }
        public bool isTrapezoidal { get; set; }

        int[] nodes;
        double compResistance;
        double curSourceValue;

        public Inductor() : base()
        {
            nodes = new int[2];
            inductance = 1;
        }

        public Inductor(double induc) : base()
        {
            nodes = new int[2];
            inductance = induc;
        }

        public override void reset()
        {
            current = lead_volt[0] = lead_volt[1] = 0;
        }

        public override void stamp(Circuit sim)
        {
            nodes[0] = lead_node[0];
            nodes[1] = lead_node[1];
            if (isTrapezoidal)
            {
                compResistance = 2 * inductance / sim.timeStep;
            }
            else
            {
                compResistance = inductance / sim.timeStep; // backward euler
            }
            sim.stampResistor(nodes[0], nodes[1], compResistance);
            sim.stampRightSide(nodes[0]);
            sim.stampRightSide(nodes[1]);
        }

        public override void beginStep(Circuit sim)
        {
            double voltdiff = lead_volt[0] - lead_volt[1];
            if (isTrapezoidal)
            {
                curSourceValue = voltdiff / compResistance + current;
            }
            else
            {
                curSourceValue = current; // backward euler
            }
        }

        public override bool nonLinear() { return true; }

        public override void calculateCurrent()
        {
            double voltdiff = lead_volt[0] - lead_volt[1];
            if (compResistance > 0)
                current = voltdiff / compResistance + curSourceValue;
        }

        public override void step(Circuit sim)
        {
            double voltdiff = lead_volt[0] - lead_volt[1];
            sim.stampCurrentSource(nodes[0], nodes[1], curSourceValue);
        }

        /*public override void getInfo(String[] arr) {
			arr[0] = "inductor";
			getBasicInfo(arr);
			arr[3] = "L = " + getUnitText(inductance, "H");
			arr[4] = "P = " + getUnitText(getPower(), "W");
		}*/

    }
}