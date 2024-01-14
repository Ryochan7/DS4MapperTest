using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.SteamControllerLibrary;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;
using Newtonsoft.Json;

namespace DS4MapperUnitTests
{
    [TestClass]
    public class MappingTests : BindingHelperBase
    {
        private string xinputProfileJson;
        private string darkMessiahProfileJson;
        //private TestMapper mapper;

        public MappingTests()
        {
            #region profileJSON
            xinputProfileJson = @"{
  ""Name"": ""XInput"",
  ""Description"": ""X360"",
  ""Creator"": ""ryochan7"",
  ""CreationDate"": ""2022-04-07T16:23:39+0000"",
  ""ActionSets"": [
    {
      ""Index"": ""0"",
      ""Name"": ""Set 1"",
      ""Description"": ""Only ActionSets"",
      ""ActionLayers"": [
        {
          ""Index"": ""0"",
          ""Name"": ""Default"",
          ""Description"": ""Only Action Layer"",
          ""MappedActions"": [
            {
              ""Id"": ""0"",
              ""Name"": ""LS Action"",
              ""ActionMode"": ""TouchStickTranslateAction"",
              ""Settings"":
              {
                ""OutputStick"": ""X360_LS"",
                ""DeadZone"": 0.05,
                ""AntiDeadZone"": 0.35,
                ""MaxZone"": 0.7,
                ""Rotation"": 0
              }
            },
            {
              ""Id"": ""1"",
              ""Name"": ""RS Action"",
              ""ActionMode"": ""TouchMouseJoystickAction"",
              ""Settings"":
              {
                ""OutputStick"": ""X360_RS"",
              }
            },
            {
              ""Id"": ""2"",
              ""Name"": ""DPad Action"",
              ""ActionMode"": ""DPadTranslateAction"",
              ""Settings"":
              {
                ""OutputDPad"": ""X360_DPAD""
              }
            },
            {
              ""Id"": ""3"",
              ""Name"": ""X360 A Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_A""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""4"",
              ""Name"": ""X360 B Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_B""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""5"",
              ""Name"": ""X360 X Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_X""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""6"",
              ""Name"": ""X360 Y Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_Y""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""7"",
              ""Name"": ""X360 LB Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_LB""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""8"",
              ""Name"": ""X360 RB Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_RB""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""9"",
              ""Name"": ""X360 LT Button"",
              ""ActionMode"": ""TriggerTranslateAction"",
              ""Settings"":
              {
                ""OutputTrigger"": ""X360_LT""
              }
            },
            {
              ""Id"": ""10"",
              ""Name"": ""X360 RT Button"",
              ""ActionMode"": ""TriggerTranslateAction"",
              ""Settings"":
              {
                ""OutputTrigger"": ""X360_RT""
              }
            },
            {
              ""Id"": ""11"",
              ""Name"": ""X360 Back Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_Back""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""12"",
              ""Name"": ""X360 Start Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_Start""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""13"",
              ""Name"": ""X360 ThumbL Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_ThumbL""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""14"",
              ""Name"": ""X360 ThumbR Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_ThumbR""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""15"",
              ""Name"": ""X"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_X""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""16"",
              ""Name"": ""A"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_A""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""17"",
              ""Name"": ""StickDPAD"",
              ""ActionMode"": ""StickPadAction"",
              ""Bindings"":
              {
                ""Up"":
                {
                    ""Name"": ""Dpad Up"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_UP""
                          }
                        ]
                      }
                    ]
                },
                ""Down"":
                {
                    ""Name"": ""Dpad Down"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_DOWN""
                          }
                        ]
                      }
                    ]
                },
                ""Left"":
                {
                    ""Name"": ""Dpad Left"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_LEFT""
                          }
                        ]
                      }
                    ]
                },
                ""Right"":
                {
                    ""Name"": ""Dpad Right"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_RIGHT""
                          }
                        ]
                      }
                    ]
                },
              },
              ""Settings"":
              {
                ""DeadZone"": 0.3,
                ""DiagonalRange"": 45,
              }
            },
            {
              ""Id"": ""18"",
              ""Name"": ""X360 Start Button"",
              ""ActionMode"": ""TriggerButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_Start""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""19"",
              ""Name"": ""TouchPAD"",
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"":
              {
                ""Up"":
                {
                    ""Name"": ""Dpad Up"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_UP""
                          }
                        ]
                      }
                    ]
                },
                ""Down"":
                {
                    ""Name"": ""Dpad Down"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_DOWN""
                          }
                        ]
                      }
                    ]
                },
                ""Left"":
                {
                    ""Name"": ""Dpad Left"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_LEFT""
                          }
                        ]
                      }
                    ]
                },
                ""Right"":
                {
                    ""Name"": ""Dpad Right"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_RIGHT""
                          }
                        ]
                      }
                    ]
                },
              },
              ""Settings"":
              {
                ""DeadZone"": 0.05,
                ""DiagonalRange"": 30,
              }
            },
            {
              ""Id"": ""20"",
              ""Name"": ""LS Action"",
              ""ActionMode"": ""StickTranslateAction"",
              ""Settings"":
              {
                ""OutputStick"": ""X360_LS"",
                ""DeadZone"": 0.10,
                ""AntiDeadZone"": 0.24,
                ""Rotation"": 0
              }
            }
          ]
        },
        {
          ""Index"": ""1"",
          ""Name"": ""Test Layer"",
          ""Description"": ""Action Layer"",
          ""MappedActions"": [
            {
              ""Id"": ""0"",
              ""Name"": ""X360 X Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_X""
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
      ""ActionSet"": ""0"",
      ""ActionLayer"": ""0"",
      ""InputMappings"": [
        {
          ""Input"": ""LeftTouchpad"",
          ""Action"": ""0""
        },
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": ""1""
        },
        {
          ""Input"": ""A"",
          ""Action"": ""3""
        },
        {
          ""Input"": ""B"",
          ""Action"": ""4""
        },
        {
          ""Input"": ""X"",
          ""Action"": ""5""
        },
        {
          ""Input"": ""Y"",
          ""Action"": ""6""
        },
        {
          ""Input"": ""LeftGrip"",
          ""Action"": ""15""
        },
        {
          ""Input"": ""RightGrip"",
          ""Action"": ""16""
        },
        {
          ""Input"": ""LShoulder"",
          ""Action"": ""7""
        },
        {
          ""Input"": ""RShoulder"",
          ""Action"": ""8""
        },
        {
          ""Input"": ""LT"",
          ""Action"": ""9""
        },
        {
          ""Input"": ""RT"",
          ""Action"": ""10""
        },
        {
          ""Input"": ""Back"",
          ""Action"": ""11""
        },
        {
          ""Input"": ""Start"",
          ""Action"": ""12""
        },
        {
          ""Input"": ""LeftPadClick"",
          ""Action"": ""13""
        },
        {
          ""Input"": ""RightPadClick"",
          ""Action"": ""14""
        },
        {
          ""Input"": ""Stick"",
          ""Action"": ""17""
        }
      ]
    },
    {
      ""ActionSet"": ""0"",
      ""ActionLayer"": ""1"",
      ""InputMappings"": [
        {
          ""Input"": ""Y"",
          ""Action"": ""0""
        }
      ]
    }
  ]
}

";
            darkMessiahProfileJson = @"{
  ""Name"": ""Dark Messiah"",
  ""Description"": ""Dark Messiah"",
  ""Creator"": ""ryochan7"",
  ""CreationDate"": ""2022-04-07T11:23:39-05:00"",
  ""OutputGamepadSettings"": {
    ""Enabled"": false
  },
  ""ActionSets"": [
    {
      ""Index"": 0,
      ""Name"": ""Set 1"",
      ""Description"": ""Only ActionSets"",
      ""ActionLayers"": [
        {
          ""Index"": 0,
          ""Name"": ""Default"",
          ""Description"": ""Only Action Layer"",
          ""MappedActions"": [
            {
              ""Id"": 0,
              ""Name"": ""LS Action"",
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Name"": ""Dpad Up"",
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
                  ""Name"": ""Dpad Up"",
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
                  ""Name"": ""Dpad Up"",
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
                  ""Name"": ""Dpad Up"",
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
                ""DiagonalRange"": 35,
                ""Rotation"": 0,
                ""OuterRingRange"": ""OnlyActive""
              }
            },
            {
              ""Id"": 1,
              ""Name"": ""RS Action"",
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""DeadZone"": 6,
                ""TrackballFriction"": 7,
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0,
                ""SmoothingMinCutoff"": 0.8,
                ""SmoothingBeta"": 0.6
              }
            },
            {
              ""Id"": 2,
              ""Name"": ""DPad Action"",
              ""ActionMode"": ""DPadTranslateAction"",
              ""Settings"": {
                ""OutputDPad"": ""X360_DPAD""
              }
            },
            {
              ""Id"": 3,
              ""Name"": ""A Button"",
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
              ""Id"": 4,
              ""Name"": ""X360 B Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""C""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 5,
              ""Name"": ""X360 X Button"",
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
            },
            {
              ""Id"": 6,
              ""Name"": ""X360 Y Button"",
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
              ""Id"": 7,
              ""Name"": ""X360 LB Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""LeftAlt""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 8,
              ""Name"": ""Devil Trigger"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""V""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 9,
              ""Name"": ""X360 LT Button"",
              ""ActionMode"": ""TriggerDualStageAction"",
              ""SoftPull"": {
                ""Name"": ""Soft Pull"",
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
                ]
              },
              ""Settings"": {
                ""DeadZone"": 0.2,
                ""MaxZone"": 1.0,
                ""AntiDeadZone"": 0.0,
                ""DualStageMode"": ""Threshold"",
                ""HipFireDelay"": 0,
                ""ForceHipFireDelay"": false
              }
            },
            {
              ""Id"": 10,
              ""Name"": ""X360 RT Button"",
              ""ActionMode"": ""TriggerDualStageAction"",
              ""SoftPull"": {
                ""Name"": ""Soft Pull"",
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
                ]
              },
              ""Settings"": {
                ""DeadZone"": 0.2,
                ""MaxZone"": 1.0,
                ""AntiDeadZone"": 0.0,
                ""DualStageMode"": ""Threshold"",
                ""HipFireDelay"": 0,
                ""ForceHipFireDelay"": false
              }
            },
            {
              ""Id"": 11,
              ""Name"": ""X360 Back Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""Tab""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 12,
              ""Name"": ""X360 Start Button"",
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
              ""Id"": 13,
              ""Name"": ""X360 ThumbL Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""LeftShift""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 14,
              ""Name"": ""X360 ThumbR Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""LeftControl""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 15,
              ""Name"": ""Use Quick Slots"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""HoldActionLayer"",
                      ""Layer"": 1
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 16,
              ""Name"": ""Jump"",
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
              ""Id"": 17,
              ""Name"": ""Stick Mouse Wheel"",
              ""ActionMode"": ""StickPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": []
                },
                ""Down"": {
                  ""Functions"": []
                },
                ""Left"": {
                  ""Name"": ""Dpad Left"",
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
                ""Right"": {
                  ""Name"": ""Dpad Right"",
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
                }
              },
              ""Settings"": {
                ""DeadZone"": 0.3,
                ""DiagonalRange"": 45
              }
            },
            {
              ""Id"": 18,
              ""Name"": ""X360 Start Button"",
              ""ActionMode"": ""TriggerButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_Start""
                    }
                  ]
                }
              ],
              ""Settings"": {
                ""DeadZone"": 0.11764705882352941
              }
            },
            {
              ""Id"": 19,
              ""Name"": ""TouchPAD"",
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Name"": ""Dpad Up"",
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""GamepadControl"",
                          ""PadOutput"": ""X360_DPAD_UP""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {
                  ""Name"": ""Dpad Down"",
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""GamepadControl"",
                          ""PadOutput"": ""X360_DPAD_DOWN""
                        }
                      ]
                    }
                  ]
                },
                ""Left"": {
                  ""Name"": ""Dpad Left"",
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""GamepadControl"",
                          ""PadOutput"": ""X360_DPAD_LEFT""
                        }
                      ]
                    }
                  ]
                },
                ""Right"": {
                  ""Name"": ""Dpad Right"",
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""GamepadControl"",
                          ""PadOutput"": ""X360_DPAD_RIGHT""
                        }
                      ]
                    }
                  ]
                }
              },
              ""Settings"": {
                ""DeadZone"": 0.05,
                ""DiagonalRange"": 30,
                ""OuterRingRange"": ""OnlyActive""
              }
            },
            {
              ""Id"": 20,
              ""Name"": ""LS Action"",
              ""ActionMode"": ""StickTranslateAction"",
              ""Settings"": {
                ""OutputStick"": ""X360_LS"",
                ""DeadZone"": 0.1,
                ""AntiDeadZone"": 0.24,
                ""Rotation"": 0
              }
            }
          ]
        },
        {
          ""Index"": 1,
          ""Name"": ""Quick Slots"",
          ""Description"": ""Slots"",
          ""MappedActions"": [
            {
              ""Id"": 0,
              ""Name"": ""A Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""N7""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 1,
              ""Name"": ""X360 B Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""N6""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 2,
              ""Name"": ""X360 X Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""N8""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 3,
              ""Name"": ""X360 Y Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""N5""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 4,
              ""Name"": ""RightTouchpad Spells"",
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Name"": ""Slot 1"",
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""N1""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {
                  ""Name"": ""Slot 3"",
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""N3""
                        }
                      ]
                    }
                  ]
                },
                ""Left"": {
                  ""Name"": ""Slot 4"",
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""N4""
                        }
                      ]
                    }
                  ]
                },
                ""Right"": {
                  ""Name"": ""Slot 2"",
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""N2""
                        }
                      ]
                    }
                  ]
                }
              },
              ""Settings"": {
                ""DeadZone"": 0.2,
                ""PadMode"": ""FourWayCardinal"",
                ""DiagonalRange"": 30,
                ""Rotation"": 0,
                ""OuterRingRange"": ""OnlyActive""
              }
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
          ""Action"": 3
        },
        {
          ""Input"": ""B"",
          ""Action"": 4
        },
        {
          ""Input"": ""X"",
          ""Action"": 5
        },
        {
          ""Input"": ""Y"",
          ""Action"": 6
        },
        {
          ""Input"": ""LShoulder"",
          ""Action"": 7
        },
        {
          ""Input"": ""RShoulder"",
          ""Action"": 8
        },
        {
          ""Input"": ""LT"",
          ""Action"": 9
        },
        {
          ""Input"": ""RT"",
          ""Action"": 10
        },
        {
          ""Input"": ""Back"",
          ""Action"": 11
        },
        {
          ""Input"": ""Start"",
          ""Action"": 12
        },
        {
          ""Input"": ""LeftPadClick"",
          ""Action"": 13
        },
        {
          ""Input"": ""RightPadClick"",
          ""Action"": 14
        },
        {
          ""Input"": ""LeftGrip"",
          ""Action"": 15
        },
        {
          ""Input"": ""RightGrip"",
          ""Action"": 16
        },
        {
          ""Input"": ""Stick"",
          ""Action"": 17
        }
      ]
    },
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 1,
      ""InputMappings"": [
        {
          ""Input"": ""A"",
          ""Action"": 0
        },
        {
          ""Input"": ""B"",
          ""Action"": 1
        },
        {
          ""Input"": ""X"",
          ""Action"": 2
        },
        {
          ""Input"": ""Y"",
          ""Action"": 3
        },
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 4
        }
      ]
    }
  ]
}";
            #endregion

            mapper = new TestMapper();
        }

        [TestMethod]
        public void KeyboardMouseTests()
        {
            Profile tempProfile = new Profile();
            // Need to create mapper using profile
            mapper = new TestMapper(tempProfile);
            // Purge default action set data
            tempProfile.ActionSets.Clear();
            List<ProfileActionsMapping> tempMappings = null;

            ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);

            // Check that profile can be read at all
            string json = darkMessiahProfileJson;
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

            // Populate input state struct with mock data
            SteamControllerState inputState = new SteamControllerState()
            {
                A = true,
                LeftPad = new SteamControllerState.TouchPadInfo()
                {
                    X = 32767,
                    Y = 32767,
                    Touch = true,
                    Click = false,
                },
            };

            {
                // Run mapper routine and check output state
                mapper.Reader_Report(inputState, out IntermediateState _);
                Assert.AreEqual(true, TestMapper.KeyReferenceCountDict.ContainsKey(44)); // A (Space)
                Assert.AreEqual(true, TestMapper.KeyReferenceCountDict.ContainsKey(7)); // LPad X (D)
                Assert.AreEqual(true, TestMapper.KeyReferenceCountDict.ContainsKey(26)); // LPad Y (W)
            }

            // Populate input state struct with mock data
            inputState = new SteamControllerState()
            {
                A = false,
                LeftPad = new SteamControllerState.TouchPadInfo()
                {
                    X = 0,
                    Y = 32767,
                    Touch = true,
                    Click = false,
                },
            };

            {
                // Run mapper routine and check output state
                mapper.Reader_Report(inputState, out IntermediateState _);
                Assert.AreEqual(false, TestMapper.KeyReferenceCountDict.ContainsKey(44)); // A (Space)
                Assert.AreEqual(false, TestMapper.KeyReferenceCountDict.ContainsKey(7)); // LPad X (D)
                Assert.AreEqual(true, TestMapper.KeyReferenceCountDict.ContainsKey(26)); // LPad Y (W)
            }
        }

        [TestMethod]
        public void XInputOutputTests()
        {
            Profile tempProfile = new Profile();
            // Need to create mapper using profile
            mapper = new TestMapper(tempProfile);
            // Purge default action set data
            tempProfile.ActionSets.Clear();
            List<ProfileActionsMapping> tempMappings = null;

            ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);

            // Check that profile can be read at all
            string json = xinputProfileJson;
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

            // Populate input state struct with mock data
            SteamControllerState inputState = new SteamControllerState()
            {
                A = true,
                // Move stick to DownRight
                LX = 27000,
                LY = -27000,
            };

            {
                // Run mapper routine and check output state
                mapper.Reader_Report(inputState, out IntermediateState outputState);
                Assert.AreEqual(true, outputState.BtnSouth); // A
                Assert.AreEqual(true, outputState.DpadDown); // LY
                Assert.AreEqual(true, outputState.DpadRight); // LX
            }

            // Populate input state struct with mock data
            inputState = new SteamControllerState()
            {
                X = true,
                LeftPad = new SteamControllerState.TouchPadInfo()
                {
                    X = 32767,
                    Y = 0,
                    Touch = true,
                    Click = false,
                },
            };

            {
                // Run mapper routine and check output state
                mapper.Reader_Report(inputState, out IntermediateState outputState);
                Assert.AreEqual(true, outputState.BtnWest); // X
                Assert.AreEqual(false, outputState.BtnSouth); // A (check release)
                Assert.AreEqual(1.0, outputState.LX); // LeftPad X
                Assert.AreEqual(0.0, outputState.LY); // LeftPad Y
                Assert.AreEqual(false, outputState.BtnThumbL); // LeftPad Click
            }
        }
    }
}
