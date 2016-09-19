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
echo "<img src=\"".$actual_link.":8090/?action=stream\" width=\"640\" height=\"480\"/>";

echo "<br>"; 

echo "<a href=\"".$actual_link.":8090/?action=stream\" target=\"_blank\">";
echo "<input type=\"Submit\" value=\"Stream\" /></a>";

echo "\r\n";

//echo "<a href=\"camerasettings.php\" target=\"_blank\">";
//echo "<input type=\"Submit\" value=\"Controls\" /></a>";

echo "\r\n";

echo "<a href=\"index.php\" target=\"_self\">";
echo "<input type=\"Submit\" value=\"Back\" /></a>";
?>

<button onclick="myFunction()">Settings</button>

<script>
function myFunction() {
    window.open("camerasettings.php",'window','toolbar=no, menubar=no, resizable=yes, width=680, height=300');
}
</script>

</body>
</html>

