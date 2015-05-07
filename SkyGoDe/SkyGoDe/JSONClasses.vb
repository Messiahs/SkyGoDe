Imports SkyGoDe.PluginConfiguration
Imports System.Runtime.Serialization

Namespace SkyGoDe
    Public Class JSONClasses
        '/thanks to http://jsonutils.com/
        Public Class JSONLettersClass
            Public Class Letter
                Public Property linkable As Boolean
                Public Property content As Object
            End Class

            Public Class Letters
                Public Property records As Integer
                Public Property letter As Letter()
            End Class

            Public Class JSONObject
                Public Property letters As Letters
                Public Property product As String
            End Class
        End Class

        Public Class JSONChannelListClass
            Public Class ChannelList
                Public Property id As String
                Public Property skygoid As String
                Public Property enable As Boolean
                Public Property name As String
                Public Property color As String
                Public Property hd As Integer
                Public Property logo As String
                Public Property num As Integer
                Public Property mobilepc As String
                Public Property ppv As Integer
                Public Property mediaurl As String
                Public Property mastTemplateUrl As String
                Public Property advChannelId As String
            End Class

            Public Class JSONObject
                Public Property channelList As ChannelList()
            End Class
        End Class

        Public Class JSONEventDetailClass
            Public Class TechIcons
                Public Property hd As Integer
                Public Property v3d As Integer
                Public Property multiLang As Integer
                Public Property sound As Integer
                Public Property ut As Integer
                Public Property serie As Integer
                Public Property pin As Integer
                Public Property lastChance As Integer
            End Class

            Public Class DeviceIcons
                Public Property tv As Integer
                Public Property web As Integer
                Public Property iPad As Integer
                Public Property iPhone As Integer
                Public Property xBox As Integer
            End Class

            Public Class JSONObject
                Public Property title1 As String
                Public Property title2 As String
                Public Property color As String
                Public Property detailTxt As String
                Public Property category As String
                Public Property fsk As String
                Public Property fskInfo As String
                Public Property country As String
                Public Property year As String
                Public Property record As Integer
                Public Property detailPage As String
                Public Property imageUrl As String
                Public Property priceDE As String
                Public Property priceAT As String
                Public Property taxDE As String
                Public Property taxAT As String
                Public Property license As String
                Public Property assetid As String
                Public Property cmsid As String
                Public Property catalog As String
                Public Property techIcons As TechIcons
                Public Property deviceIcons As DeviceIcons
                Public Property returnCode As ResponseCode
            End Class
        End Class

        Public Class JSONLiveEventListClass
            Public Class LiveEventList
                Public Property id As String
                Public Property startDate As String
                Public Property startTime As String
                Public Property endDate As String
                Public Property endTime As String
                Public Property length As Integer
                Public Property title As String
                Public Property subtitle As String
                Public Property live As Integer
                Public Property isNew As Integer
                Public Property highlight As Integer
            End Class
        End Class

        Public Class JSONClass
            Public Class PictureInfo
                Public Property path As String
                Public Property file As String
            End Class
            Public Class PictureInfos
                Public Property picture As PictureInfo()
            End Class
            Public Class ParentalRating
                Public Property value As Integer
                Public Property display As String
            End Class
            Public Class Other
                Public Property id As Integer
                Public Property content As String
            End Class
            Public Class Category
                Public Property main As Main
                Public Property other As Other()
            End Class
            Public Class Main
                Public Property id As Integer
                Public Property apix_id As String
                Public Property content As String
            End Class
            Public Class OnAir
                Public Property start_date As String
                Public Property end_date As String
            End Class
            Public Class SocialGroupIds
                Public Property social As Object()
            End Class
            Public Class License
                Public Property internet As Integer
                Public Property mobile As Integer
            End Class
            Public Class Asset
                Public Property id As String
                Public Property type As String
                Public Property hd As String
                Public Property downloadable As String
                Public Property current_type As String
                Public Property title As String
                Public Property original_title As String
                Public Property path As String
                Public Property logo As String
                Public Property nr As String
                Public Property episode_nr As Integer
                Public Property season_nr As Integer
                Public Property content As String
                Public Property serie_id As String
                Public Property season_id As String
                Public Property serie_title As String
                Public Property apix_id As String
                Public Property event_id As String
                Public Property [alias] As String
                Public Property subtitle As String
                Public Property audio As String
                Public Property synopsis As String
                Public Property country As Object
                Public Property cast_list As Object
                Public Property parental_rating As ParentalRating
                Public Property year_of_production As Integer
                Public Property year_of_production_start As Integer
                Public Property year_of_production_end As Integer
                Public Property package_code As String
                Public Property package_color_code As String
                Public Property lenght As Integer
                Public Property media_url As String
                Public Property ms_media_url As String
                Public Property genre As Object
                Public Property category As Category
                Public Property asset_type As String
                Public Property dvd_cover As PictureInfo
                Public Property dvd_cover_jpg As PictureInfo
                Public Property main_picture As PictureInfos
                Public Property main_trailer As Object
                Public Property on_air As OnAir
                Public Property highlight As String
                Public Property channel_name As String
                Public Property channel_code As String
                Public Property flag_live As String
                Public Property flag_simulcast As String
                Public Property flag_series As String
                Public Property license As License
                Public Property webvod_canonical_url As String
                Public Property socialGroupIds As SocialGroupIds
                Public Property webplayer_config As Object
                Public Property season_apix_id As String
                Public Property series_apix_id As String
                Public Property mast_url As String
                Public Property coverSmall_localPath As String
                Public Property coverSmall_url As String
                Public Property coverBig_localPath As String
                Public Property coverBig_url As String
                Public Property backgroundImage_localPath As String
                Public Property backgroundImage_url As String
                Public Property Description As String
                Public Property VideoUrl As String
            End Class
            Public Class Episodes
                Public Property count As Integer
                Public Property episode As Asset()
            End Class
            Public Class Season
                Inherits Asset
                Public Property episodes As Episodes
            End Class
            Public Class Seasons
                Public Property count As Integer
                Public Property season As Season()
            End Class
            Public Class Serie
                Inherits Asset
                Public Property seasons As Seasons
            End Class
            Public Class SerieSummary
                Public Property serie As Serie
            End Class
            Public Class Assets
                Public Property records As Integer
                Public Property asset As Asset()
            End Class
            Public Class Listing
                Public Property id As String
                Public Property type As String
                Public Property isPaginated As Boolean
                Public Property currPage As Integer
                Public Property maxPageSize As Integer
                Public Property pages As Integer
                Public Property currentLetter As String
                Public Property asset_listing As Assets
            End Class
            Public Class SerieObject
                Public Property serieRecap As SerieSummary
                Public Property product As String
            End Class
            Public Class AdditionalDataObject
                Public Property asset As Asset
                Public Property product As String
            End Class
            Public Class VideoListingObject
                Public Property listing As Listing
                Public Property product As String
            End Class
        End Class

    End Class
End Namespace