I made this basic graphics that shows what the current logic is doing (based on the constraints that I could see in the given engine API). I hope it brings clarity.

**Step 1**: Cast a ray from A to based
**Step 2**: Find a collision in this ray (and it's one at a time)
**Step 3**: Find the exit point of the object. The reason this is important is because bullet penetration will be determined by the penetration power of the bullet and the material it's hitting. If the bullet "runs out of power" to penetrate the object it'll stop without exiting. But if it does sustain power it'll come out in step 4
**Step 4**: Continue forward momentum to point B. If it collides with another object repeat steps 2-4
**Step 5**: Fin, and the collision layer flag for all objects involved is reverted

Important detail regarding these steps: at step 2 I change the collision layer of the object (this graphic shows Layer 2, but it's an arbitrary decision what layer to use). This allows for step 3 to be possible because then I can limit my collision shape to what's on layer 2. Once this one bullet has finished all penetration logic all collided objects have their collision layer flag reverted.