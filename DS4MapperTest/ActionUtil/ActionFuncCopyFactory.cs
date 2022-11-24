
namespace DS4MapperTest.ActionUtil
{
    public static class ActionFuncCopyFactory
    {
        public static ActionFunc CopyFunc(ActionFunc func)
        {
            ActionFunc result = null;

            switch(func)
            {
                case NormalPressFunc normPress:
                    {
                        NormalPressFunc temp = new NormalPressFunc(normPress);
                        result = temp;
                        break;
                    }
                case DistanceFunc distFunc:
                    {
                        DistanceFunc temp = new DistanceFunc(distFunc);
                        result = temp;
                        break;
                    }
                case DoublePressFunc doublePressFunc:
                    {
                        DoublePressFunc temp = new DoublePressFunc(doublePressFunc);
                        result = temp;
                        break;
                    }
                case HoldPressFunc holdFunc:
                    {
                        HoldPressFunc temp = new HoldPressFunc(holdFunc);
                        result = temp;
                        break;
                    }
                case ReleaseFunc releaseFunc:
                    {
                        ReleaseFunc temp = new ReleaseFunc(releaseFunc);
                        result = temp;
                        break;
                    }
                case StartPressFunc startFunc:
                    {
                        StartPressFunc temp = new StartPressFunc(startFunc);
                        result = temp;
                        break;
                    }
                default: break;
            }

            return result;
        }
    }
}
