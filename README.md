Size of Keylog for 10 hours at an average of 80 WPM


#m-record
##Key Logger

### Memory Usage Calculation

**Assumptions:**
- Typing speed: **80 words per minute (WPM)**
- Session duration: **10 hours**
- Average characters per word (including space): **5**
- Each keystroke generates one CSV row

**Calculations:**

| Parameter                | Value         |
|--------------------------|--------------|
| Words per minute         | 80           |
| Minutes in 10 hours      | 600          |
| Total words              | 48,000       |
| Total keystrokes         | 240,000      |
| Estimated bytes per row  | 80           |
| Total bytes              | 19,200,000   |
| Total megabytes (MB)     | ~18.3 MB     |

- **Total keystrokes:** 80 WPM × 600 min × 5 chars = **240,000**
- **Estimated bytes per row:** 40 characters × 2 bytes (UTF-16) = **80 bytes**
- **Total memory usage:** 240,000 rows × 80 bytes = **19,200,000 bytes** (~18.3 MB)

### Summary

After 10 hours of recording at 80 WPM, the keystroke log will use approximately **18–20 MB** of memory. 
This is well within the capabilities of modern systems. For longer sessions or higher rates, I might consider periodically saving the log to disk.
