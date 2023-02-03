using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

using Photon.Pun;
using Photon.Realtime;

namespace FreeDraw
{
    [RequireComponent(typeof(SpriteRenderer))] // REQUIRES A COLLIDER2D to function
    // 1. Attach this to a read/write enabled sprite image
    // 2. Set the drawing_layers  to use in the raycast
    // 3. Attach a 2D collider (like a Box Collider 2D) to this sprite
    // 4. Hold down left mouse to draw on this texture!
    public class Drawable : MonoBehaviour, IMixedRealityFocusHandler, IMixedRealityPointerHandler
    {
        // PEN COLOUR
        public static Color32 Pen_Colour = Color.gray;     // Change these to change the default drawing settings
        // PEN WIDTH (actually, it's a radius, in pixels)
        public static int Pen_Width = 3;

        public int Eraser_Width = 6;
        public int TimesDrawn = 0;

        Color32 Saved_Colour;
        int Saved_Width;


        public delegate void Brush_Function(Vector3 local_pos);
        // This is the function called when a left click happens
        // Pass in your own custom one to change the brush type
        // Set the default function in the Awake method
        public Brush_Function current_brush;

        public LayerMask Drawing_Layers;

        public bool Reset_Canvas_On_Play = true;
        public bool EnableVRDraw;

        // The colour the canvas is reset to each time
        public Color Reset_Colour = new Color(0, 0, 0, 0);  // By default, reset the canvas to be transparent

        // Used to reference THIS specific file without making all methods static
        public static Drawable drawable;
        // MUST HAVE READ/WRITE enabled set in the file editor of Unity
        Sprite drawable_sprite;
        Texture2D drawable_texture;

        Camera _camera;

        Vector2 previous_drag_position;
        Color[] clean_colours_array;
        Color transparent;
        Color32[] cur_colors;

        float[] colorInfo = new float[4];

        IMixedRealityPointer _pointer;
        IMixedRealityPointer[] hand_pointers;

        Color Eraser_Color;

        int numFocused = 0;
        //bool mouse_was_previously_held_down = false;
        //bool no_drawing_on_current_drag = false;

        bool isErasing = false;
        private bool isClicking = false;

        //Network enabled componenent
        PhotonView photonview;

        Vector3 startPosition;
        Quaternion startRotation;


        //////////////////////////////////////////////////////////////////////////////
        // BRUSH TYPES. Implement your own here


        // When you want to make your own type of brush effects,
        // Copy, paste and rename this function.
        // Go through each step
        public void BrushTemplate(Vector2 world_position)
        {
            // 1. Change world position to pixel coordinates
            Vector2 pixel_pos = WorldToPixelCoordinates(world_position);

            // 2. Make sure our variable for pixel array is updated in this frame
            cur_colors = drawable_texture.GetPixels32();

            ////////////////////////////////////////////////////////////////
            // FILL IN CODE BELOW HERE

            // Do we care about the user left clicking and dragging?
            // If you don't, simply set the below if statement to be:
            //if (true)

            // If you do care about dragging, use the below if/else structure
            if (previous_drag_position == Vector2.zero)
            {
                // THIS IS THE FIRST CLICK
                // FILL IN WHATEVER YOU WANT TO DO HERE
                // Maybe mark multiple pixels to colour?
                MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
            }
            else
            {
                // THE USER IS DRAGGING
                // Should we do stuff between the previous mouse position and the current one?
                ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
            }
            ////////////////////////////////////////////////////////////////

            // 3. Actually apply the changes we marked earlier
            // Done here to be more efficient
            ApplyMarkedPixelChanges();

            // 4. If dragging, update where we were previously
            previous_drag_position = pixel_pos;
        }




        // Default brush type. Has width and colour.
        // Pass in a point in WORLD coordinates
        // Changes the surrounding pixels of the world_point to the static pen_colour

        public void PenBrush(Vector3 local_pos)
        {

            

            Vector2 pixel_pos = LocalToPixelCoordinates(local_pos);

            cur_colors = drawable_texture.GetPixels32();

            if (previous_drag_position == Vector2.zero)
            {
                // If this is the first time we've ever dragged on this image, simply colour the pixels at our mouse position
                MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
            }
            else
            {
                // Colour in a line from where we were on the last update call
                ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
            }
            ApplyMarkedPixelChanges();

            //Debug.Log("Dimensions: " + pixelWidth + "," + pixelHeight + ". Units to pixels: " + unitsToPixels + ". Pixel pos: " + pixel_pos);
            previous_drag_position = pixel_pos;
        }

        [PunRPC]
        public void PenBrushNet(Vector3 local_pos, int _Pen_Width, float[] colorBuild, Vector2 _previous_drag_position)
        {

            Color _Pen_Colour = new Color(colorBuild[0], colorBuild[1], colorBuild[2], colorBuild[3]);

            Vector2 pixel_pos = LocalToPixelCoordinates(local_pos);

            cur_colors = drawable_texture.GetPixels32();

            if (_previous_drag_position == Vector2.zero)
            {
                // If this is the first time we've ever dragged on this image, simply colour the pixels at our mouse position
                MarkPixelsToColour(pixel_pos, _Pen_Width, _Pen_Colour);
            }
            else
            {
                // Colour in a line from where we were on the last update call
                ColourBetween(_previous_drag_position, pixel_pos, _Pen_Width, _Pen_Colour);
            }
            ApplyMarkedPixelChanges();

            //Debug.Log("Dimensions: " + pixelWidth + "," + pixelHeight + ". Units to pixels: " + unitsToPixels + ". Pixel pos: " + pixel_pos);
        }


        // Helper method used by UI to set what brush the user wants
        // Create a new one for any new brushes you implement
        public void SetPenBrush()
        {
            // PenBrush is the NAME of the method we want to set as our current brush
            current_brush = PenBrush;
        }
        //////////////////////////////////////////////////////////////////////////////


        // This is where the magic happens.
        // Detects when user is [click action] and hovering over whiteboard, which then call the appropriate function

        void Update()
        {

            

            if (EnableVRDraw && numFocused > 0)
            {



                //Debug.Log(_pointer.PointerName);
                //Debug.Log(_pointer.PointerId);
                //Debug.Log("Position" + position.x);
                if (_pointer.Result != null)
                {
                    //Debug.Log("Pointer exists");
                    if (_pointer.IsFocusLocked)
                    {

                        Vector3 local_pos = WorldToLocalCoordinates(_pointer.Result.Details.Point);

                        //should be parallelized in future with target.other so we can send in current colors and width as well
                        photonview.RPC("PenBrushNet", RpcTarget.Others, local_pos, Pen_Width, colorInfo, previous_drag_position);
                        current_brush(local_pos);
                        //Debug.Log("Attempted to draw");
                    }
                    else
                    {

                        previous_drag_position = Vector2.zero;

                    }
                }
            }
            else
            {
                //Debug.Log("None focused");
            }


            /*
            // Is the user holding down the left mouse button?
            bool mouse_held_down = Input.GetMouseButton(0);
            if (mouse_held_down && !no_drawing_on_current_drag)
            {
                // Convert mouse coordinates to world coordinates
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if(Physics.Raycast(ray, out RaycastHit hit, 100.0f, Drawing_Layers.value))
                {

                    current_brush(hit.point);

                }

                else
                {
                    // We're not over our destination texture
                    previous_drag_position = Vector2.zero;
                    if (!mouse_was_previously_held_down)
                    {
                        // This is a new drag where the user is left clicking off the canvas
                        // Ensure no drawing happens until a new drag is started
                        no_drawing_on_current_drag = true;
                    }
                }
            }
            // Mouse is released
            else if (!mouse_held_down)
            {
                previous_drag_position = Vector2.zero;
                no_drawing_on_current_drag = false;
            }
            mouse_was_previously_held_down = mouse_held_down;
            */
        }



        // Set the colour of pixels in a straight line from start_point all the way to end_point, to ensure everything inbetween is coloured
        public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color32 color)
        {
            
            
            // Get the distance from start to finish
            float distance = Vector2.Distance(start_point, end_point);
            Vector2 direction = (start_point - end_point).normalized;

            Vector2 cur_position = start_point;

            // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
            float lerp_steps = 1/ distance; //CHANGE:: added width to calculation, interpolation and frame check needed

            //OPTIMIZATION:: this should be changed to account for font size and only draw on area a previous interpolation has not yet reached

            for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
            {
                cur_position = Vector2.Lerp(start_point, end_point, lerp);
                MarkPixelsToColour(cur_position, width, color);
            }
            
        }





        public void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color32 color_of_pen)
        {
            // Figure out how many pixels we need to colour in each direction (x and y)
            int center_x = (int)center_pixel.x;
            int center_y = (int)center_pixel.y;
            //int extra_radius = Mathf.Min(0, pen_thickness - 2);
            TimesDrawn++;
            for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
            {
                // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
                if (x >= (int)drawable_sprite.rect.width || x < 0)
                    continue;

                for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
                {
                    MarkPixelToChange(x, y, color_of_pen);
                }
            }
        }
        public void MarkPixelToChange(int x, int y, Color32 color)
        {
            // Need to transform x and y coordinates to flat coordinates of array
            int array_pos = y * (int)drawable_sprite.rect.width + x;

            // Check if this is a valid position
            if (array_pos > cur_colors.Length || array_pos < 0)
                return;

            cur_colors[array_pos].r = color.r;
            cur_colors[array_pos].g = color.g;
            cur_colors[array_pos].b = color.b;
        }
        public void ApplyMarkedPixelChanges()
        {
            drawable_texture.SetPixels32(cur_colors);
            drawable_texture.Apply();
        }


        // Directly colours pixels. This method is slower than using MarkPixelsToColour then using ApplyMarkedPixelChanges
        // SetPixels32 is far faster than SetPixel
        // Colours both the center pixel, and a number of pixels around the center pixel based on pen_thickness (pen radius)
        public void ColourPixels(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
        {
            // Figure out how many pixels we need to colour in each direction (x and y)
            int center_x = (int)center_pixel.x;
            int center_y = (int)center_pixel.y;
            //int extra_radius = Mathf.Min(0, pen_thickness - 2);

            for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
            {
                for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
                {
                    drawable_texture.SetPixel(x, y, color_of_pen);
                }
            }

            drawable_texture.Apply();
        }


        public Vector3 WorldToLocalCoordinates(Vector3 world_position)
        {

            return transform.InverseTransformPoint(world_position);

        }


        public Vector2 LocalToPixelCoordinates(Vector3 local_pos)
        {

            //NEEDED:: fixes for scalability
            // Change coordinates to local coordinates of this image

            // Change these to coordinates of pixels
            float pixelWidth = drawable_sprite.rect.width;
            float pixelHeight = drawable_sprite.rect.height;

            float unitsToPixels = pixelWidth / drawable_sprite.bounds.size.x;  // * transform.localScale.x; WHY was this here in the first place??

            // Need to center our coordinates
            float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
            float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

            // Round current mouse position to nearest pixel
            Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

            return pixel_pos;
        }


        public Vector2 WorldToPixelCoordinates(Vector2 world_position)
        {

            Vector3 local_pos = transform.InverseTransformPoint(world_position);

            // Change these to coordinates of pixels
            float pixelWidth = drawable_sprite.rect.width;
            float pixelHeight = drawable_sprite.rect.height;

            float unitsToPixels = pixelWidth / drawable_sprite.bounds.size.x * transform.localScale.x;

            // Need to center our coordinates
            float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
            float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

            // Round current mouse position to nearest pixel
            Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

            return pixel_pos;

        }


        // Changes every pixel to be the reset colour
        [PunRPC]
        public void ResetCanvas()
        {
            drawable_texture.SetPixels(clean_colours_array);
            drawable_texture.Apply();
        }


        
        void Awake()
        {
            drawable = this;
            // DEFAULT BRUSH SET HERE
            current_brush = PenBrush;

            photonview = PhotonView.Get(this);

            _camera = Camera.main;

            drawable_sprite = this.GetComponent<SpriteRenderer>().sprite;
            drawable_texture = drawable_sprite.texture;

            startPosition = this.transform.position;
            startRotation = this.transform.rotation;

            colorInfo[0] = Pen_Colour.r;
            colorInfo[1] = Pen_Colour.g;
            colorInfo[2] = Pen_Colour.b;
            colorInfo[3] = 1;

            // Initialize clean pixels to use
            clean_colours_array = new Color[(int)drawable_sprite.rect.width * (int)drawable_sprite.rect.height];
            for (int x = 0; x < clean_colours_array.Length; x++)
                clean_colours_array[x] = Reset_Colour;

            Eraser_Color = Reset_Colour;
            // Should we reset our canvas image when we hit play in the editor?
            if (Reset_Canvas_On_Play)
                ResetCanvas();
        }


        #region VR Implemented Methods

        //Changes state to determine if gaze is available/on this drawable object.
        public void EnterGazeOnWhiteBoard(Microsoft.MixedReality.Toolkit.Input.FocusEventData eventData)
        {


            //set focus to true because the pointer is on the object
            numFocused += 1;
            _pointer = eventData.Pointer;
            //Debug.Log("In focus");

        }

        public void EnterGazeOnWhiteBoard(Microsoft.MixedReality.Toolkit.Input.HandTrackingInputEventData eventData)
        {

            numFocused += 1;
            hand_pointers = eventData.InputSource.Pointers;


        }

        public void ExitGazeOnWhiteBoard(FocusEventData eventData)
        {
            //set focus to false because pointer has left the object
            numFocused -= 1;
            //Debug.Log("Out of focus");

        }

        #endregion


        #region Pen Color Controls(MRTK)

        //sets the pen color to the value on the 'r' slider
        public void SetPenColorRed(SliderEventData eventData)
        {
            if (isErasing)
            {
                ToggleErasing();
            }
            Pen_Colour = new Color(eventData.NewValue, Pen_Colour.g/255f, Pen_Colour.b/255f);
            colorInfo[0] = Pen_Colour.r;
            

        }

        //sets the pen color to the value on the 'g' slider
        public void SetPenColorGreen(SliderEventData eventData)
        {
            if (isErasing)
            {
                ToggleErasing();
            }
            Pen_Colour = new Color(Pen_Colour.r/255f, eventData.NewValue, Pen_Colour.b/255f);
            colorInfo[1] = Pen_Colour.g;
            

        }

        //sets the pen color to the value on the 'b' slider
        public void SetPenColorBlue(SliderEventData eventData)
        {
            if (isErasing)
            {
                ToggleErasing();
            }
            Pen_Colour = new Color(Pen_Colour.r/255f, Pen_Colour.g/255f, eventData.NewValue);
            colorInfo[2] = Pen_Colour.b;
            

        }
        #endregion

        #region Font Size Controls

        public void SetFontSize(int fontSize)
        {
            //Debug.Log("Receiving changes");
            Pen_Width = fontSize;

        }


        #endregion

        #region IMixedRealityFocus

        void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
        {

            EnterGazeOnWhiteBoard(eventData);
            if (EnableVRDraw)
            {
                eventData.Pointer.IsTargetPositionLockedOnFocusLock = false;
            }
        }


        void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
        {
            eventData.Pointer.IsTargetPositionLockedOnFocusLock = true;
            ExitGazeOnWhiteBoard(eventData);

        }


        #endregion


        #region IMixedRealityPointerHandler

        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            //Debug.Log("Click activated");
            _pointer = eventData.Pointer;
            isClicking = true;


        }

        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            //Debug.Log("Click deactivated");
            isClicking = false;

        }


        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
        {

        }


        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {

        }

        #endregion


        #region External Controller Functions

        public void ToggleDrawingEnabled()
        {
            Debug.Log("Toggled Drawing!");
            EnableVRDraw = !EnableVRDraw;
        }


        public void ToggleErasing()
        {

            if (!isErasing)
            {
                isErasing = true;

                Saved_Colour = Pen_Colour;
                Saved_Width = Pen_Width;

                Pen_Colour = Eraser_Color;
                Pen_Width = Eraser_Width;

                colorInfo[0] = Eraser_Color.r;
                colorInfo[1] = Eraser_Color.g;
                colorInfo[2] = Eraser_Color.b;
                colorInfo[3] = Eraser_Color.a;

            }
            else
            {
                isErasing = false;

                Pen_Colour = Saved_Colour;
                Pen_Width = Saved_Width;

                colorInfo[0] = Pen_Colour.r;
                colorInfo[1] = Pen_Colour.g;
                colorInfo[2] = Pen_Colour.b;
                colorInfo[3] = Pen_Colour.a;

            }

        }


        public void ResetPosition()
        {

            this.transform.position = startPosition;
            this.transform.rotation = startRotation;


        }


        #endregion


        #region PUN Implemented Methods

        public void WipeAllCanvas()
        {

            photonview.RPC("ResetCanvas", RpcTarget.All);

        }


        #endregion

    }
}