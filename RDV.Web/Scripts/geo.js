function Geo() {
    this.getPolygonCenter = function(coords){
        var center = [];
        var sumX = 0;
        var sumY = 0;
        for (var i = 0; i < coords.length; i++) {
            sumX += parseFloat(coords[i][0]);
            sumY += parseFloat(coords[i][1]);
        }
        center[0] = sumX / coords.length;
        center[1] = sumY / coords.length;
        return center;
    };

    this.serializeCoordinates = function(coords){
        var str = "";
        $.each(coords[0], function(index, item) {
            str += item[0] + "," + item[1] + "|";
        });
        return str;
    };

    this.parseCoordinates = function(str){
        var result = [];
        var pairs = str.split('|');
        if (pairs.length <= 1) {
            result = [[48.4914, 135.041],[48.4625, 135.072],[48.4799, 135.098],[48.5021, 135.063],[48.4914, 135.041]];
        } else {
            $.each(pairs, function(index, item) {
                var coords = item.split(",");
                if (coords.length == 2) {
                    var coordItem = [coords[0], coords[1]];
                    result.push(coordItem);
                }
            });
        }
        return result;
    };
}
var geo = new Geo();