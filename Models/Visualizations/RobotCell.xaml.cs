﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace SafetySharp.CaseStudies.Visualizations
{
    using CaseStudies.RobotCell.Modeling;
    using CaseStudies.RobotCell.Modeling.Controllers;
    using Modeling;
    using Odp;
    using Odp.Reconfiguration;

    /// <summary>
    /// Interaktionslogik für RobotCell.xaml
    /// </summary>
    public partial class RobotCell
    {
        private Model _model;
        private ModelBuilder _builder;

        private readonly Dictionary<uint, RobotControl> _robots = new Dictionary<uint, RobotControl>();
        private readonly Dictionary<uint, CartControl> _carts = new Dictionary<uint, CartControl>();
        private readonly List<WorkpieceControl> _workpieces = new List<WorkpieceControl>();

        private List<string> _task;
        private List<string> doneCapabilities = new List<string>();
        private ListBox lboxTask = new ListBox();
        private List<TextBox> routes = new List<TextBox>();
        private int resourceCounter;

        public RobotCell()
        {
            //Initialize task-box
            lboxTask.Visibility = Visibility.Visible;
            lboxTask.SelectionMode = SelectionMode.Multiple;

            
            //Ictss6 manually, because of problems with the SampleModels-file
            _builder = new ModelBuilder("model builder");
            _builder.DefineTask(5, ModelBuilderHelper.Produce, ModelBuilderHelper.Drill, ModelBuilderHelper.Insert, ModelBuilderHelper.Tighten, ModelBuilderHelper.Consume);

            InitializeComponent();
            
            SimulationControls.ModelStateChanged += (o, e) => UpdateModelState();
            SimulationControls.Reset += (o, e) => OnModelStateReset();

            _builder.AddRobot(ModelBuilderHelper.Produce, ModelBuilderHelper.Insert);
            _builder.AddRobot(ModelBuilderHelper.Insert);
            _builder.AddRobot(ModelBuilderHelper.Drill, ModelBuilderHelper.Tighten);
            _builder.AddRobot(ModelBuilderHelper.Tighten);
            _builder.AddRobot(ModelBuilderHelper.Drill, ModelBuilderHelper.Consume);
            
            _builder.AddCart(ModelBuilderHelper.Route(0, 1), ModelBuilderHelper.Route(0, 2));
            _builder.AddCart(ModelBuilderHelper.Route(1, 3), ModelBuilderHelper.Route(1, 4), ModelBuilderHelper.Route(1, 2), ModelBuilderHelper.Route(2, 4));
            
            _builder.ChooseController<FastController>();
            _builder.CentralReconfiguration();
            
            Model = _builder.Build();
            Model.Faults.SuppressActivations();
            
            //Initialize/Update task
            _task = UpdateTask();
            PrintTask();

            //Initialize resource counter
            resourceCounter = _model.Resources.Count;

            SimulationControls.MaxSpeed = 64;
            SimulationControls.ChangeSpeed(1);
        }

        public Model Model
        {
            get { return _model; }
            set
            {
                _model = value;
                InitializeVisualization();
                SimulationControls.SetModel(value);
            }
        }

        private void InitializeVisualization()
        {
            var robotCount = _model.RobotAgents.Count;

            //Create grid for robots and carts
            double widthToHeightRatio = 2;
            var gridHeigth = Math.Max((int)Math.Ceiling(Math.Sqrt(robotCount/ widthToHeightRatio)), 1);
            var gridWidth = (int)(widthToHeightRatio * gridHeigth);

            //Console.Out.WriteLine("<HEIGHT> " + gridHeigth + "  <WIDTH> " + gridWidth);

            visualizationArea.RowDefinitions.Clear();
            visualizationArea.ColumnDefinitions.Clear();

            for (int i = 0; i < gridHeigth; i++) {
                //CreateRow(1);
                CreateRow(3);
                CreateRow(2);
            }
            for (int i = 0; i < gridWidth; i++) {
                //CreateColumn(1);
                CreateColumn(3);
                CreateColumn(2);
            }

            //Create and place robots
            for (int i = 0; i < robotCount; i++) {
                var robot = new RobotControl(_model.RobotAgents[i], this);
                _robots.Add(_model.RobotAgents[i].Id, robot);
                visualizationArea.Children.Add(robot);

                var row = 2 * (i / gridWidth);
                var col = 2 * (i % gridWidth);
                
                Grid.SetRow(robot, row);
                Grid.SetColumn(robot, col);
            }
            
            //Create and place carts
            for (int i = 0; i < _model.CartAgents.Count; i++)
            {
                var agent = _model.CartAgents[i];
                var cart = new CartControl(agent, this);
                _carts.Add(agent.Id, cart);
                visualizationArea.Children.Add(cart);

                var robot = _model.RobotAgents.First(r => agent.Cart.IsPositionedAt(r.Robot));
                int row, column;
                GetFreePosition(robot, out row, out column);
                
                row += 2;
                column += 2;

                Grid.SetRow(cart, row);
                Grid.SetColumn(cart, column);
            }

            //Create textboxes for displaying the carts' routes
            for (int i = 0; i < _carts.Count; i++)
            {
                routes.Add(new TextBox());
            }

            //Create resources for numbers larger than one

            //for (int i = 0; i < _model.Resources.Count; i++) {
            //    var agent = _model.Resources[i];
            //    var resource = new WorkpieceControl();
            //    _workpieces.Add(resource);
            //}

            //Create resource (currently only one)
            var resource = new WorkpieceControl();
            _workpieces.Add(resource);

            //Display task 
            _task = UpdateTask();
            DisplayTask();

            //Place Workpiece/Resource
            PlaceWorkpiece();

            //Draw routes for the carts
            //DrawRoutes();
            PrintCartRoutes();

            //Display the current role of a robot
            DisplayRole();
        }

        private void DisplayRole() {
            foreach (var robot in _robots) {
                var agent = robot.Value.GetRobotAgent();
                Console.WriteLine("'ROLE HAS VALUE' IS: "+agent.RoleExecutor.Role.HasValue);
            }
        }

        private void PlaceWorkpiece() {

            foreach (var robot in _robots)
            {
                var agent = robot.Value.GetRobotAgent();

                if (agent.HasResource)
                    robot.Value.workpiece.Visibility = Visibility.Visible;
                else
                    robot.Value.workpiece.Visibility = Visibility.Hidden;
            }

            foreach (var cart in _carts) {
                var agent = cart.Value.GetCartAgent();

                if (agent.HasResource)
                    cart.Value.workpiece.Visibility = Visibility.Visible;
                else
                    cart.Value.workpiece.Visibility = Visibility.Hidden;
            }
        }

        internal void GetFreePosition(RobotAgent robot, out int row, out int col) {
            var ctrl = _robots[robot.Id];
            var robotRow = Grid.GetRow(ctrl);
            var robotColumn = Grid.GetColumn(ctrl);

            var pos = new[] { robotRow - 1, robotRow, robotRow + 1 }
                .Zip(new[] { robotColumn - 1, robotColumn, robotColumn + 1 }, Tuple.Create)
                .First(coords => IsFreePosition(coords.Item1, coords.Item2));
            row = pos.Item1;
            col = pos.Item2;
        }

        private bool IsFreePosition(int row, int col) {
            return !visualizationArea.Children.Cast<UIElement>().Any(ctrl => Grid.GetRow(ctrl) == row && Grid.GetColumn(ctrl) == col);
        }

        private void OnModelStateReset()
        {
            
            _model = (Model)SimulationControls.Model;

            if (SimulationControls.Simulator.IsReplay)
                return;

            foreach (var robot in _robots) {
                robot.Value.Update(robot.Value.GetRobotAgent());
            }
                        
            //Reset the selected task capabilities
            ClearTaskDisplay();
        }

        private void UpdateModelState()
        {
            //Check if a new resource is processed
            if (resourceCounter != _model.Resources.Count)
            {
                ClearTaskDisplay();
                resourceCounter--;
            }

            foreach (var robot in _model.RobotAgents) {
                _robots[robot.Id].Update(robot);
            }

            foreach (var cart in _model.CartAgents) {
                _carts[cart.Id].Update(cart);
            }

            PlaceWorkpiece();

            _task = UpdateTask();
            PrintTask();
            DisplayTask();
            UpdateTaskSelection();

            PrintCartRoutes();

            InvalidateArrange();
            InvalidateVisual();
            UpdateLayout();

            visualizationArea.InvalidateArrange();
            visualizationArea.InvalidateVisual();
        }

        public void CreateRow(double height) {
            var row = new RowDefinition() { Height = new GridLength(height, GridUnitType.Star) }; 
            visualizationArea.RowDefinitions.Add(row);
        }

        public void CreateColumn(double width)
        {
            var col = new ColumnDefinition() { Width = new GridLength(width, GridUnitType.Star) };
            visualizationArea.ColumnDefinitions.Add(col);
        }

        internal static string GetState(BaseAgent agent) {
            var agentType = typeof(BaseAgent);
            var machineField = agentType.GetField("_stateMachine", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var machine = machineField.GetValue(agent);

            var machineType = machine.GetType();
            var stateField = machineType.GetProperty("State");
            return stateField.GetValue(machine).ToString();
        }

        //Currently it could be called "InitializeTask", but it's built for eventually more workpieces, thus more tasks
        private List<string> UpdateTask()
        {
            Task task = new Task();
            if (_model.Resources.Count > 0)
                task = (Task)_model.Resources.First().Task;
            ICapability[] capabilities = task.RequiredCapabilities;
            List<string> stringCapabilities = new List<string>();

            foreach (var cap in capabilities)
            {
                if (cap.GetType() == typeof(ProduceCapability) || cap.GetType() == typeof(ConsumeCapability))
                {
                    if (!stringCapabilities.Contains(cap.CapabilityType.ToString()))
                        stringCapabilities.Add(cap.CapabilityType.ToString());
                }

                if (cap.GetType() == typeof(ProcessCapability))
                {
                    var proc = (ProcessCapability)cap;
                    if (!stringCapabilities.Contains(proc.ProductionAction.ToString()))
                        stringCapabilities.Add(proc.ProductionAction.ToString());
                }
            }

            return stringCapabilities;
        }

        private void PrintTask()
        {
            string str = "Task: ";
            for (int i = 0; i < _task.Count; i++)
            {
                if (i < _task.Count - 1)
                    str += _task.ElementAt(i) + ", ";
                else
                    str += _task.ElementAt(i);
            }
            Console.WriteLine(str);
        }

        private void DisplayTask() {
            
            if (!lboxTask.Items.Contains("Current Task:"))
                lboxTask.Items.Add("Current Task:");

            foreach (var cap in _task)
            {
                if (!lboxTask.Items.Contains(cap))
                    lboxTask.Items.Add(cap);
            }

            var row = visualizationArea.RowDefinitions.Count - 2;
            var col = visualizationArea.ColumnDefinitions.Count - 2;

            if (IsFreePosition(row, col)) {
                visualizationArea.Children.Add(lboxTask);
                Grid.SetColumn(lboxTask, col);
                Grid.SetRow(lboxTask, row);
            }         
        }

        public void UpdateTaskSelection() {
            foreach (var cap in doneCapabilities)
            {

                if (!lboxTask.SelectedItems.Contains(cap))
                    lboxTask.SelectedItems.Add(cap);

                Console.WriteLine("\nSelected Items: ");

                foreach (var sel in lboxTask.SelectedItems)
                {
                    Console.WriteLine("Selected Item = " + sel);
                }
                Console.WriteLine();

                if (_task.Count > 0)
                    if (cap == _task.ElementAt(_task.Count - 2))
                    {
                        Console.WriteLine("COMPLETED!!!");
                        break;
                    }
            }
        }

        public List<string> GetDoneCapabilities() {
            return doneCapabilities;
        }

        public void AddDoneCapability(string cap) {
            if (!doneCapabilities.Contains(cap))
                doneCapabilities.Add(cap);
        }

        public void PrintCartRoutes()
        {
            int j = 0;
            foreach (var cart in _carts.Values)
            {
                TextBox routeBox = routes.ElementAt((int)cart.GetCartAgent().Id % _carts.Count);

                Grid.SetColumn(routeBox, Grid.GetColumn(cart) - 1);
                Grid.SetRow(routeBox, Grid.GetRow(cart));
                if (!visualizationArea.Children.Contains(routeBox))
                    visualizationArea.Children.Add(routeBox);
                routeBox.Visibility = Visibility.Visible;

                var agent = cart.GetCartAgent();
                var cartPlant = agent.Cart;

                var routesPlant = cartPlant.Routes;
                int i = 0;

                routeBox.Text = "Routes:\n";
                Console.WriteLine();
                
                foreach (var route in routesPlant)
                {
                    var nameR1 = route.Robot1.Name;
                    var nameR2 = route.Robot2.Name;

                    if ((i + j) % 3 == 0)
                        routeBox.Text += cart.CreateRouteString(nameR1, nameR2) + "\n";
                    else
                        routeBox.Text += cart.CreateRouteString(nameR1, nameR2);

                    i++;
                    j++;
                }

                routeBox.IsReadOnly = true;
            }            
        }

        public void ClearTaskDisplay()
        {
            for (int i = doneCapabilities.Count - 1; i >= 0; i--)
            {
                doneCapabilities.RemoveAt(i);
            }
            lboxTask.SelectedItems.Clear();
        }
    }
}