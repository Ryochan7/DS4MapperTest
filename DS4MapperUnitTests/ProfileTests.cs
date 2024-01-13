using Newtonsoft.Json;
using DS4MapperTest;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;

namespace DS4MapperUnitTests
{
    [TestClass]
    public class ProfileTests : BindingHelperBase
    {
        private string daggerFallProfileStr;
        private string daggerFallGyroMouseProfileStr;
        //private TestMapper mapper;

        public ProfileTests()
        {
            #region profileJSON
            daggerFallProfileStr = @"{
  ""Name"": ""Daggerfall"",
  ""Description"": ""Daggerfall"",
  ""Creator"": ""ryochan7"",
  ""CreationDate"": ""2023-10-27T02:05:40.4042828Z"",
  ""OutputGamepadSettings"": {
    ""Enabled"": false
  },
  ""LightbarSettings"": {
    ""Mode"": ""SolidColor"",
    ""SolidColor"": {
      ""red"": 0,
      ""green"": 0,
      ""blue"": 255
    },
    ""RainbowSecondsCycle"": 5
  },
  ""ActionSets"": [
    {
      ""Index"": 0,
      ""Name"": ""Main"",
      ""ActionLayers"": [
        {
          ""Index"": 0,
          ""Name"": ""Default"",
          ""MappedActions"": [
            {
              ""Id"": 0,
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""W""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""S""
                        }
                      ]
                    }
                  ]
                },
                ""Left"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""A""
                        }
                      ]
                    }
                  ]
                },
                ""Right"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""D""
                        }
                      ]
                    }
                  ]
                }
              },
              ""Settings"": {
                ""DeadZone"": 0.05,
                ""OuterRingRange"": ""OnlyActive""
              }
            },
            {
              ""Id"": 1,
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""DeadZone"": 8,
                ""TrackballFriction"": 8,
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0,
                ""SmoothingEnabled"": true,
                ""SmoothingMinCutoff"": 2.0,
                ""SmoothingBeta"": 0.7
              }
            },
            {
              ""Id"": 2,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""Space""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 3,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""M""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 4,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""Escape""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 5,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""HoldActionLayer"",
                      ""Layer"": 2
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 6,
              ""ActionMode"": ""TriggerButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""MouseButton"",
                      ""Code"": ""RightButton""
                    }
                  ]
                }
              ],
              ""Settings"": {
                ""DeadZone"": 0.11764705882352941
              }
            },
            {
              ""Id"": 7,
              ""ActionMode"": ""TriggerButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""MouseButton"",
                      ""Code"": ""LeftButton""
                    }
                  ]
                }
              ],
              ""Settings"": {
                ""DeadZone"": 0.11764705882352941
              }
            },
            {
              ""Id"": 8,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""LeftShift""
                    }
                  ],
                  ""Settings"": {
                    ""Toggle"": true
                  }
                }
              ]
            },
            {
              ""Id"": 9,
              ""ActionMode"": ""StickPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F1""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F3""
                        }
                      ]
                    }
                  ]
                },
                ""Left"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F4""
                        }
                      ]
                    }
                  ]
                },
                ""Right"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F2""
                        }
                      ]
                    }
                  ]
                }
              },
              ""Settings"": {
                ""DeadZone"": 0.5,
                ""PadMode"": ""FourWayCardinal"",
                ""DeadZoneType"": ""Radial""
              }
            },
            {
              ""Id"": 10,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""ApplyActionLayer"",
                      ""Layer"": 1,
                      ""Settings"": {
                        ""ChangeCondition"": ""Released""
                      }
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 11,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""X""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 12,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""E""
                    }
                  ]
                }
              ]
            }
          ]
        },
        {
          ""Index"": 1,
          ""Name"": ""Mouse Wheel"",
          ""MappedActions"": [
            {
              ""Id"": 0,
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0
              }
            },
            {
              ""Id"": 1,
              ""ActionMode"": ""TouchCircularAction"",
              ""Clockwise"": {
                ""Functions"": [
                  {
                    ""Type"": ""NormalPress"",
                    ""OutputActions"": [
                      {
                        ""Type"": ""MouseWheel"",
                        ""Code"": ""WheelDown""
                      }
                    ]
                  }
                ]
              },
              ""CounterClockwise"": {
                ""Functions"": [
                  {
                    ""Type"": ""NormalPress"",
                    ""OutputActions"": [
                      {
                        ""Type"": ""MouseWheel"",
                        ""Code"": ""WheelUp""
                      }
                    ]
                  }
                ]
              },
              ""Settings"": {
                ""Sensitivity"": 1.0
              }
            },
            {
              ""Id"": 10,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""RemoveActionLayer"",
                      ""Layer"": 1,
                      ""Settings"": {
                        ""ChangeCondition"": ""Released""
                      }
                    }
                  ]
                }
              ]
            }
          ]
        },
        {
          ""Index"": 2,
          ""Name"": ""Meta"",
          ""MappedActions"": [
            {
              ""Id"": 3,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""HoldPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F9""
                    }
                  ],
                  ""Settings"": {
                    ""HoldTime"": 100
                  }
                }
              ]
            },
            {
              ""Id"": 4,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""HoldPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F11""
                    }
                  ],
                  ""Settings"": {
                    ""HoldTime"": 100
                  }
                }
              ]
            },
            {
              ""Id"": 2,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F6""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 5,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""RemoveActionLayer"",
                      ""Layer"": 2,
                      ""Settings"": {
                        ""ChangeCondition"": ""Released""
                      }
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 0,
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0
              }
            },
            {
              ""Id"": 1,
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Empty""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {},
                ""Left"": {},
                ""Right"": {}
              },
              ""Settings"": {
                ""DeadZone"": 0.05,
                ""OuterRingRange"": ""OnlyActive""
              }
            },
            {
              ""Id"": 11,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Empty""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 6,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F""
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    }
  ],
  ""Mappings"": [
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 0,
      ""InputMappings"": [
        {
          ""Input"": ""LeftTouchpad"",
          ""Action"": 0
        },
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 1
        },
        {
          ""Input"": ""A"",
          ""Action"": 2
        },
        {
          ""Input"": ""Back"",
          ""Action"": 3
        },
        {
          ""Input"": ""Start"",
          ""Action"": 4
        },
        {
          ""Input"": ""Steam"",
          ""Action"": 5
        },
        {
          ""Input"": ""LT"",
          ""Action"": 6
        },
        {
          ""Input"": ""RT"",
          ""Action"": 7
        },
        {
          ""Input"": ""LShoulder"",
          ""Action"": 8
        },
        {
          ""Input"": ""Stick"",
          ""Action"": 9
        },
        {
          ""Input"": ""RightGrip"",
          ""Action"": 10
        },
        {
          ""Input"": ""B"",
          ""Action"": 11
        },
        {
          ""Input"": ""X"",
          ""Action"": 12
        }
      ]
    },
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 1,
      ""InputMappings"": [
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 1
        },
        {
          ""Input"": ""RightGrip"",
          ""Action"": 10
        }
      ]
    },
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 2,
      ""InputMappings"": [
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 1
        },
        {
          ""Input"": ""A"",
          ""Action"": 2
        },
        {
          ""Input"": ""Back"",
          ""Action"": 3
        },
        {
          ""Input"": ""Start"",
          ""Action"": 4
        },
        {
          ""Input"": ""Steam"",
          ""Action"": 5
        },
        {
          ""Input"": ""Y"",
          ""Action"": 6
        },
        {
          ""Input"": ""B"",
          ""Action"": 11
        }
      ]
    }
  ]
}";

            daggerFallGyroMouseProfileStr = @"{
  ""Name"": ""Daggerfall"",
  ""Description"": ""Daggerfall"",
  ""Creator"": ""ryochan7"",
  ""CreationDate"": ""2023-10-27T02:05:40.4042828Z"",
  ""OutputGamepadSettings"": {
    ""Enabled"": false
  },
  ""LightbarSettings"": {
    ""Mode"": ""SolidColor"",
    ""SolidColor"": {
      ""red"": 0,
      ""green"": 0,
      ""blue"": 255
    },
    ""RainbowSecondsCycle"": 5
  },
  ""ActionSets"": [
    {
      ""Index"": 0,
      ""Name"": ""Main"",
      ""ActionLayers"": [
        {
          ""Index"": 0,
          ""Name"": ""Default"",
          ""MappedActions"": [
            {
              ""Id"": 0,
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""W""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""S""
                        }
                      ]
                    }
                  ]
                },
                ""Left"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""A""
                        }
                      ]
                    }
                  ]
                },
                ""Right"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""D""
                        }
                      ]
                    }
                  ]
                }
              },
              ""Settings"": {
                ""DeadZone"": 0.05,
                ""OuterRingRange"": ""OnlyActive""
              }
            },
            {
              ""Id"": 1,
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""DeadZone"": 8,
                ""TrackballFriction"": 8,
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0,
                ""SmoothingEnabled"": true,
                ""SmoothingMinCutoff"": 2.0,
                ""SmoothingBeta"": 0.7
              }
            },
            {
              ""Id"": 2,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""Space""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 3,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""M""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 4,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""Escape""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 5,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""HoldActionLayer"",
                      ""Layer"": 2
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 6,
              ""ActionMode"": ""TriggerButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""MouseButton"",
                      ""Code"": ""RightButton""
                    }
                  ]
                }
              ],
              ""Settings"": {
                ""DeadZone"": 0.11764705882352941
              }
            },
            {
              ""Id"": 7,
              ""ActionMode"": ""TriggerButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""MouseButton"",
                      ""Code"": ""LeftButton""
                    }
                  ]
                }
              ],
              ""Settings"": {
                ""DeadZone"": 0.11764705882352941
              }
            },
            {
              ""Id"": 8,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""LeftShift""
                    }
                  ],
                  ""Settings"": {
                    ""Toggle"": true
                  }
                }
              ]
            },
            {
              ""Id"": 9,
              ""ActionMode"": ""StickPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F1""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F3""
                        }
                      ]
                    }
                  ]
                },
                ""Left"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F4""
                        }
                      ]
                    }
                  ]
                },
                ""Right"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F2""
                        }
                      ]
                    }
                  ]
                }
              },
              ""Settings"": {
                ""DeadZone"": 0.5,
                ""PadMode"": ""FourWayCardinal"",
                ""DeadZoneType"": ""Radial""
              }
            },
            {
              ""Id"": 10,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""ApplyActionLayer"",
                      ""Layer"": 1,
                      ""Settings"": {
                        ""ChangeCondition"": ""Released""
                      }
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 11,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""X""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 12,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""E""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 13,
              ""ActionMode"": ""GyroMouseAction"",
              ""Settings"": {
                ""DeadZone"": 10,
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0,
                ""InvertX"": false,
                ""InvertY"": false,
                ""TriggerButtons"": ""AlwaysOn"",
                ""TriggerActivates"": true,
                ""EvalCond"": ""And"",
                ""UseForXAxis"": ""Yaw"",
                ""MinThreshold"": 0.0,
                ""Toggle"": false
              }
            }
          ]
        },
        {
          ""Index"": 1,
          ""Name"": ""Mouse Wheel"",
          ""MappedActions"": [
            {
              ""Id"": 0,
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0
              }
            },
            {
              ""Id"": 1,
              ""ActionMode"": ""TouchCircularAction"",
              ""Clockwise"": {
                ""Functions"": [
                  {
                    ""Type"": ""NormalPress"",
                    ""OutputActions"": [
                      {
                        ""Type"": ""MouseWheel"",
                        ""Code"": ""WheelDown""
                      }
                    ]
                  }
                ]
              },
              ""CounterClockwise"": {
                ""Functions"": [
                  {
                    ""Type"": ""NormalPress"",
                    ""OutputActions"": [
                      {
                        ""Type"": ""MouseWheel"",
                        ""Code"": ""WheelUp""
                      }
                    ]
                  }
                ]
              },
              ""Settings"": {
                ""Sensitivity"": 1.0
              }
            },
            {
              ""Id"": 10,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""RemoveActionLayer"",
                      ""Layer"": 1,
                      ""Settings"": {
                        ""ChangeCondition"": ""Released""
                      }
                    }
                  ]
                }
              ]
            }
          ]
        },
        {
          ""Index"": 2,
          ""Name"": ""Meta"",
          ""MappedActions"": [
            {
              ""Id"": 3,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""HoldPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F9""
                    }
                  ],
                  ""Settings"": {
                    ""HoldTime"": 100
                  }
                }
              ]
            },
            {
              ""Id"": 4,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""HoldPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F11""
                    }
                  ],
                  ""Settings"": {
                    ""HoldTime"": 100
                  }
                }
              ]
            },
            {
              ""Id"": 2,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F6""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 5,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""RemoveActionLayer"",
                      ""Layer"": 2,
                      ""Settings"": {
                        ""ChangeCondition"": ""Released""
                      }
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 0,
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0
              }
            },
            {
              ""Id"": 1,
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Empty""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {},
                ""Left"": {},
                ""Right"": {}
              },
              ""Settings"": {
                ""DeadZone"": 0.05,
                ""OuterRingRange"": ""OnlyActive""
              }
            },
            {
              ""Id"": 11,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Empty""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 6,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F""
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    }
  ],
  ""Mappings"": [
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 0,
      ""InputMappings"": [
        {
          ""Input"": ""LeftTouchpad"",
          ""Action"": 0
        },
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 1
        },
        {
          ""Input"": ""A"",
          ""Action"": 2
        },
        {
          ""Input"": ""Back"",
          ""Action"": 3
        },
        {
          ""Input"": ""Start"",
          ""Action"": 4
        },
        {
          ""Input"": ""Steam"",
          ""Action"": 5
        },
        {
          ""Input"": ""LT"",
          ""Action"": 6
        },
        {
          ""Input"": ""RT"",
          ""Action"": 7
        },
        {
          ""Input"": ""LShoulder"",
          ""Action"": 8
        },
        {
          ""Input"": ""Stick"",
          ""Action"": 9
        },
        {
          ""Input"": ""RightGrip"",
          ""Action"": 10
        },
        {
          ""Input"": ""B"",
          ""Action"": 11
        },
        {
          ""Input"": ""X"",
          ""Action"": 12
        },
        {
          ""Input"": ""Gyro"",
          ""Action"": 13
        }
      ]
    },
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 1,
      ""InputMappings"": [
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 1
        },
        {
          ""Input"": ""RightGrip"",
          ""Action"": 10
        }
      ]
    },
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 2,
      ""InputMappings"": [
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 1
        },
        {
          ""Input"": ""A"",
          ""Action"": 2
        },
        {
          ""Input"": ""Back"",
          ""Action"": 3
        },
        {
          ""Input"": ""Start"",
          ""Action"": 4
        },
        {
          ""Input"": ""Steam"",
          ""Action"": 5
        },
        {
          ""Input"": ""Y"",
          ""Action"": 6
        },
        {
          ""Input"": ""B"",
          ""Action"": 11
        }
      ]
    }
  ]
}";
            #endregion

            mapper = new TestMapper();
        }

        [TestMethod]
        public void ReadProfileData()
        {
            Profile tempProfile = new Profile();
            // Purge default action set data
            tempProfile.ActionSets.Clear();
            List<ProfileActionsMapping> tempMappings = null;
            ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);

            // Check that profile can be read at all
            string json = daggerFallProfileStr;
            JsonConvert.PopulateObject(json, profileSerializer);

            // Migrate profile data from serializer to Profile instance
            profileSerializer.PopulateProfile();
            tempProfile.ResetAliases();
            // Need list of currently bound actions
            tempMappings = profileSerializer.ActionMappings;

            // Map actions to dictionary and mapping string ID
            FillMappingProfileInitialData(tempProfile, tempMappings);
            // Sync action layer dictionaries
            SyncActionData(tempProfile);
        }

        [TestMethod]
        public void WriteProfileJSON()
        {
            Profile tempProfile = new Profile();
            // Purge default action set data
            tempProfile.ActionSets.Clear();
            List<ProfileActionsMapping> tempMappings = null;

            ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);

            // Check that profile can be read at all
            string json = daggerFallProfileStr;
            JsonConvert.PopulateObject(json, profileSerializer);

            // Migrate profile data from serializer to Profile instance
            profileSerializer.PopulateProfile();
            tempProfile.ResetAliases();
            // Need list of currently bound actions
            tempMappings = profileSerializer.ActionMappings;

            // Map actions to dictionary and mapping string ID
            FillMappingProfileInitialData(tempProfile, tempMappings);
            // Sync action layer dictionaries
            SyncActionData(tempProfile);

            // Write JSON output and check against original input JSON
            string tempOutJson = JsonConvert.SerializeObject(profileSerializer, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    //Converters = new List<JsonConverter>()
                    //{
                    //    new MapActionSubTypeConverter(),
                    //}
                    //TypeNameHandling = TypeNameHandling.Objects
                    //ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            Assert.AreEqual(daggerFallProfileStr, tempOutJson);
        }

        [TestMethod]
        public void ModifyActionsTest()
        {
            Profile tempProfile = new Profile();
            // Purge default action set data
            tempProfile.ActionSets.Clear();
            List<ProfileActionsMapping> tempMappings = null;

            ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);

            // Check that profile can be read at all
            string json = daggerFallProfileStr;
            JsonConvert.PopulateObject(json, profileSerializer);

            // Migrate profile data from serializer to Profile instance
            profileSerializer.PopulateProfile();
            tempProfile.ResetAliases();
            tempMappings = profileSerializer.ActionMappings;

            // Map actions to dictionary and mapping string ID
            FillMappingProfileInitialData(tempProfile, tempMappings);
            // Sync action layer dictionaries
            SyncActionData(tempProfile);

            // Grab currently bound Gyro action and swap it with a GyroMouse action
            ActionSet currentSet = tempProfile.CurrentActionSet;
            ActionLayer currentLayer = tempProfile.CurrentActionSet.CurrentActionLayer;
            GyroMapAction oldAction = currentLayer.gyroActionDict["Gyro"];
            GyroMouse mouseAction = new GyroMouse();
            mouseAction.CopyBaseProps(oldAction);

            bool exists = currentLayer.LayerActions.Contains(oldAction);
            if (exists)
            {
                // Reuse old action ID
                mouseAction.Id = oldAction.Id;
                currentLayer.ReplaceGyroAction(oldAction, mouseAction);
            }
            else
            {
                // Need to find new ID for new action
                int tempId = currentLayer.FindNextAvailableId();
                mouseAction.Id = tempId;

                currentLayer.AddGyroAction(mouseAction);
            }

            if (currentSet.UsingCompositeLayer)
            {
                currentSet.RecompileCompositeLayer(mapper);
            }
            else
            {
                // Recompile composite layer
                currentSet.DefaultActionLayer.SyncActions();
                currentSet.ClearCompositeLayerActions();
                currentSet.PrepareCompositeLayer();
            }

            profileSerializer = new ProfileSerializer(tempProfile);
            string tempOutJson = JsonConvert.SerializeObject(profileSerializer, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    //Converters = new List<JsonConverter>()
                    //{
                    //    new MapActionSubTypeConverter(),
                    //}
                    //TypeNameHandling = TypeNameHandling.Objects
                    //ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            Assert.AreEqual(daggerFallGyroMouseProfileStr, tempOutJson);
        }
    }
}