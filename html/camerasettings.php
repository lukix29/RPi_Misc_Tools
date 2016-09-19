<!DOCTYPE html>
<style>
    body {
        background-color: black;
    }

    h1 {
        color: white;
    }

    h2 {
        color: white;
    }

    p {
        color: white;
    }
</style>
<html>
<body>

<?php
$actual_link = "http://$_SERVER[HTTP_HOST]";
//echo "<img src=\"".$actual_link.":8090/?action=stream\" width=\"640\" height=\"480\"/>";

//echo "<a href=\"camera.php\" target=\"_self\">";
//echo "<input type=\"Submit\" value=\"Back\" /></a>";
//echo "<br>";
echo "<iframe src=\"".$actual_link.":8090/control.htm\" width=\"640\" height=\"260\"</iframe>";
?>

</body>
</html>