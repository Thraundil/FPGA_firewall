using SME;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace simplePackageFilter
{

    [TopLevelInputBus]
    public interface KeypadControl : IBus {
        [InitialValue(false)]
        bool Program { get; set; }

        [InitialValue(false)]
        bool isPinSet { get; set; }

        // 4x4 bits for the digits
        ushort PIN { get; set; }
    }

    // Input class, which reads the input files and "simulates" the keypad
    public class inputSimulator : SimulationProcess {

        [OutputBus]
        public Keypad keyPressed = Scope.CreateBus<Keypad>();

        [InputBus]
        public KeypadControl keyControl;

        public inputSimulator(KeypadControl busIn)
        {
           keyControl = busIn;
        }

        short[] sample;

        public async override System.Threading.Tasks.Task Run() {
            sample = File.ReadAllLines("../../pinInput.txt")
                .Select(line => short.Parse(line))
                .ToArray();

            foreach (short s in sample) {
                // Couldn't find a pretty way to iterate over the
                // 'Keypad' properties, so here goes 10x ugly statements...

                if (!keyControl.Program) {
                    keyPressed.Key0 = (0 == s);
                    keyPressed.Key1 = (1 == s);
                    keyPressed.Key2 = (2 == s);
                    keyPressed.Key3 = (3 == s);
                    keyPressed.Key4 = (4 == s);
                    keyPressed.Key5 = (5 == s);
                    keyPressed.Key6 = (6 == s);
                    keyPressed.Key7 = (7 == s);
                    keyPressed.Key8 = (8 == s);
                    keyPressed.Key9 = (9 == s); 
                }
            
                await ClockAsync();
            }
        }
    }


    // Input class, which reads the input files and "simulates" the keypad
    public class programPinSim : SimulationProcess {

        [OutputBus]
        public KeypadControl keyControl = Scope.CreateBus<KeypadControl>();

        short[] sample;

        private int preVal = 0;

        public async override System.Threading.Tasks.Task Run() {
            sample = File.ReadAllLines("../../programmingInput.txt")
                .Select(line => short.Parse(line))
                .ToArray();

            foreach (short s in sample) {

                if (s != 0) {
                    if (s == 1 && this.preVal == 0) {
                        Console.WriteLine("PROGRAMMING PIN mode on - Keypad now deactivated");
                        keyControl.Program = true;
                    }
                    else if (keyControl.Program && s != 1) {
                        Console.Write("New PIN is:");
                        Console.WriteLine(s);
                        keyControl.PIN = (ushort)s;
                        keyControl.isPinSet = true;
                        Console.WriteLine("PROGRAMMING PIN mode off - Keypad now activated");
                        keyControl.Program  = false;
                    }
                    this.preVal = s;
                } else {
                    this.preVal = 0;
                }
                await ClockAsync();
            }
        }
    }


    public class controlProcess : SimpleProcess{

        [InputBus]
        public Keypad keyPressed;

        [InputBus]
        public KeypadControl keyControl;

        [OutputBus]
        public LedControl ledControl = Scope.CreateBus<LedControl>();

        [OutputBus]
        public MotorControl doorControl = Scope.CreateBus<MotorControl>();


        public controlProcess(Keypad busIn, KeypadControl busIn2)
        {
           keyPressed = busIn;
           keyControl = busIn2;
        }

        // Starting variables
        private int counter = 0;
        private int preVal = -1;
        private int[] pin   = new int[4] {0, 0, 0, 0}; // An Array is vhdl 'legal'
        private bool finalVal = false;
        private bool firstVals = false;
        private bool isRedLightOn = false;

        // In the case a PIN has not been set yet
        private int doorCounter = 0; // For ensuring the door is open 3 clock cycles

        // The majority of cases in 'doorHandler' is to ensure no multiple keys
        // are pressed, and that a keypress is followed (and preceeded) by a
        // '-1' (aka no keys pressed)
        private void doorHandler(int current){

            // Shuts off the red light, if on
            if (this.isRedLightOn) {
                this.isRedLightOn = false;
                ledControl.Red = false;
            }

            // The fourth (last) key pressed
            if (this.counter == 3 && this.preVal == -1 && current != -1) {
                this.pin[counter] = current;
                this.finalVal = true;
            }
            // The first 3 keys pressed
            else if (!this.firstVals && this.counter != 3 && current != -1 && this.preVal == -1) {
                this.pin[counter] = current;
                this.firstVals = true;
            }
            // The case in which the the initial key(s) pressed is MULTIPLE
            else if (this.firstVals && current != -1) {
                // The final input IS followed by another input
                // This is illegal, as it would suggest multiple buttons pressed
                this.pin[counter] = -1;
                this.firstVals = false;
            }
            // The case in which the the initial key(s) IS legit
            else if (this.firstVals && current == -1) {
                this.firstVals = false;
                Console.Write("Button pressed:");
                Console.WriteLine(this.pin[counter]);
                this.counter += 1;
            }
            // The case in which the last key pressed is MULTIPLE
            else if (this.finalVal && current != -1) {
                // The final input IS followed by another input
                // This is illegal, as it would suggest multiple buttons pressed
                this.pin[counter] = 0;
                this.finalVal = false;
            }
            // The case where the last key IS legit
            else if (this.finalVal && current == -1) {
                this.finalVal = false;
                Console.Write("Button pressed:");
                Console.WriteLine(this.pin[counter]);
                this.counter = 0;
                pinSuccess(); // Success(!)
            }
            // Ensures 'current' is always set to 'preVal' for any case
            this.preVal = current;

            // Ensures the door is open for 3 'ticks'
            keepDoorOpen();
        }

        private void keepDoorOpen() {
            if (doorControl.OpenDoor) {
                if (doorCounter == 3) {
                    doorControl.OpenDoor = false; // Closes the door
                    ledControl.Green = false; // Shuts off the LED
                    Console.WriteLine("The door is now closed");
                    doorCounter = 0;
                } else {
                    doorCounter += 1;
                }
            }
        }

        private void openDoor() {
                ledControl.Green = true;
                Console.WriteLine("The LED glows GREEN, the door is now open");
                doorControl.OpenDoor = true;

                for (int i = 0; i < 4; i++) {
                    this.pin[i] = 0;
                }
        }

        private void pinSuccess() {

            // In the case a PIN has not been set, just accept anything
            if (!(keyControl.isPinSet)) {
                openDoor();
            } else {
                ushort finalVal = (ushort)((this.pin[0] * 1000) + (this.pin[1] * 100) +
                               (this.pin[2] * 10) + this.pin[3]);
                // Compare with current PIN code
                if (finalVal == keyControl.PIN) {
                    openDoor();
                } else {
                    ledControl.Red = true;
                    this.isRedLightOn = true;
                    Console.WriteLine("The LED glows RED, nothing happens");
                    for (int i = 0; i < 4; i++) {
                        this.pin[i] = 0;
                    }
                }
            }
        }

        protected override void OnTick()
        {
            if (keyControl.Program) {
                Console.WriteLine("Reseting active PIN sequence...");
                this.counter = 0;
                for (int i = 0; i < 4; i++) {
                    this.pin[i] = 0;
                }
            }

            if      (keyPressed.Key0) {doorHandler(0);}
            else if (keyPressed.Key1) {doorHandler(1);}
            else if (keyPressed.Key2) {doorHandler(2);}
            else if (keyPressed.Key3) {doorHandler(3);}
            else if (keyPressed.Key4) {doorHandler(4);}
            else if (keyPressed.Key5) {doorHandler(5);}
            else if (keyPressed.Key6) {doorHandler(6);}
            else if (keyPressed.Key7) {doorHandler(7);}
            else if (keyPressed.Key8) {doorHandler(8);}
            else if (keyPressed.Key9) {doorHandler(9);}
            else {doorHandler(-1);}

        }
    }


    // Main
    public class Program
    {
        static void Main(string[] args)
        {
            using (var sim = new Simulation())
            {
                sim
                  .BuildCSVFile()
                  .BuildVHDL();

                byte[] arr = { 0, 48, 136, 21, 69, 131, 0, 24, 231, 253, 174, 161, 8, 0, 69, 0, 0, 52, 2, 31, 64, 0, 128, 6, 230, 22, 212, 25, 99, 74, 202, 177, 16, 121, 194, 156, 0, 119, 160, 128, 75, 200, 249, 141, 210, 78, 80, 24, 64, 252, 130, 182, 0, 0, 65, 82, 84, 73, 67, 76, 69, 32, 51, 52, 13, 10 };
                var stream = new MemoryStream(arr, 0, arr.Length);
                var reader = new BinaryReader(stream);

                Console.WriteLine("Version and header length {0}", reader.ReadByte());
                Console.WriteLine("Diff services{0}", reader.ReadByte());

                Console.WriteLine("Total length {0}", IPAddress.NetworkToHostOrder(reader.ReadInt16()));
                Console.WriteLine("ID {0}", IPAddress.NetworkToHostOrder(reader.ReadInt16()));
                Console.WriteLine("Flags and offset {0}", IPAddress.NetworkToHostOrder(reader.ReadInt16()));

                Console.WriteLine("TTL {0}", reader.ReadByte());
                Console.WriteLine("Protocol {0}", reader.ReadByte());
                Console.WriteLine("Checksum {0}", reader.ReadInt16());

                Console.WriteLine("Source IP {0}", new IPAddress((int)reader.ReadInt32()));
                Console.WriteLine("Destination IP {0}", new IPAddress((int)reader.ReadInt32()));

                var programSimulator = new programPinSim();
                var keypadSimulator  = new inputSimulator(programSimulator.keyControl);

                var control = new controlProcess(keypadSimulator.keyPressed, keypadSimulator.keyControl);

                sim.Run();
            }
        }
    }
}





