using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace nivax.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : nivax.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}",
                        "nivax");

            var group1 = new SampleDataGroup("Group-1",
                    "Introduction",
                    "",
                    "Assets/10.png",
                    "");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Love",
                    "",
                    "Assets/11.png",
                    "The English word love can refer to a variety of different feelings, states, and attitudes, ranging from pleasure (I loved that meal) to interpersonal attraction (I love my partner). It can refer to an emotion of a strong affection and personal attachment.",
                    "he English word love can refer to a variety of different feelings, states, and attitudes, ranging from pleasure (I loved that meal) to interpersonal attraction (I love my partner). It can refer to an emotion of a strong affection and personal attachment.It can also be a virtue representing human kindness, compassion, and affection—the unselfish loyal and benevolent concern for the good of another.\n\n And it may describe compassionate and affectionate actions towards other humans, one's self or animals.\n\nIn terms of interpersonal attraction, four forms of love have traditionally been distinguished, based on ancient Greek precedent: the love of kinship or familiarity (in Greek, storge), the love of friendship (philia), the love of sexual and/or romantic desire (eros), and self-emptying or divine love (agape).Modern authors have distinguished further varieties of romantic love.[6] Non-Western traditions have also distinguished variants or symbioses of these states. This diversity of uses and meanings, combined with the complexity of the feelings involved, makes love unusually difficult to consistently define, compared to other emotional states.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "Interpersonal relationship",
                    "",
                    "Assets/12.png",
                    "An interpersonal relationship is an association between two or more people that may range in duration from brief to enduring.",
                    "An interpersonal relationship is an association between two or more people that may range in duration from brief to enduring. This association may be based on inference, love, solidarity, regular business interactions, or some other type of social commitment. Interpersonal relationships are formed in the context of social, cultural and other influences. The context can vary from family or kinship relations, friendship, marriage, relations with associates, work, clubs, neighborhoods, and places of worship. They may be regulated by law, custom, or mutual agreement, and are the basis of social groups and society as a whole.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Biological basis of love",
                    "",
                    "Assets/13.png",
                    "The theory of a biological basis of love has been explored by such biological sciences as evolutionary psychology, evolutionary biology, anthropology and neuroscience.",
                    "Evolutionary psychology has proposed several explanations for love. Human infants and children are for a very long time dependent on parental help. Love has therefore been seen as a mechanism to promote mutual parental support of children for an extended time period. Another is that sexually transmitted diseases may cause, among other effects, permanently reduced fertility, injury to the fetus, and increase risks during childbirth. This would favor exclusive long-term relationships reducing the risk of contracting a STD.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Human bonding",
                    "",
                    "Assets/14.png",
                    "Human bonding is the process of development of a close, interpersonal relationship.",
                    "Human bonding is the process of development of a close, interpersonal relationship. It most commonly takes place between family members or friends,[1] but can also develop among groups such as sporting teams and whenever people spend time together. Bonding is a mutual, interactive process, and is different from simple liking.\n\nBonding typically refers to the process of attachment that develops between romantic partners, close friends, or parents and children. This bond is characterized by emotions such as affection and trust. Any two people who spend time together may form a bond. Male bonding refers to the establishment of relationships between men through shared activities that often exclude females. The term female bonding is less frequently used, but refers to the formation of close personal relationships between women.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "The Art of Loving",
                    "",
                    "Assets/15.png",
                    "The Art of Loving is a 1956 book by psychologist and social philosopher Erich Fromm, which was published as part of the World Perspectives Series edited by Ruth Nanda Anshen.",
                    "Fromm presents love as a skill that can be taught and developed. He rejects the idea of loving as something magical and mysterious that cannot be analyzed and explained, and is therefore skeptical about popular ideas such as falling in love or being helpless in the face of love. Because modern humans are alienated from each other and from nature, we seek refuge from our aloneness in romantic love and marriage. However, Fromm observes that real love is not a sentiment which can be easily indulged in by anyone. It is only through developing one's total personality to the capacity of loving one's neighbor with true humility, courage, faith and discipline that one attains the capacity to experience real love. This should be considered a rare achievement. Fromm defended these opinions also in interview with Mike Wallace when he states: love today is a relatively rare phenomenon, that we have a great deal of sentimentality; we have a great deal of illusion about love, namely as a...as something one falls in. But the question is that one cannot fall in love, really; one has to be in love. And that means that loving becomes, and the ability to love, becomes one of the most important things in life",
                    group1));
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                    "Impact",
                    "",
                    "Assets/20.png",
                    "");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Emotion",
                    "",
                    "Assets/21.png",
                    "In psychology, philosophy, and their many subsets, emotion is the generic term for subjective, conscious experience.",
                    "In psychology, philosophy, and their many subsets, emotion is the generic term for subjective, conscious experience that is characterized primarily by psychophysiological expressions, biological reactions, and mental states. Emotion is often associated and considered reciprocally influential with mood, temperament, personality, disposition, and motivation,[citation needed] as well as influenced by hormones and neurotransmitters such as dopamine, noradrenaline, serotonin, oxytocin, cortisol and GABA. Emotion is often the driving force behind motivation, positive or negative.An alternative definition of emotion is a positive or negative experience that is associated with a particular pattern of physiological activity.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Affection",
                    "",
                    "Assets/22.png",
                    "Affection or fondness is a disposition or rare state of mind or body that is often associated with a feeling or type of love. It has given rise to a number of branches of philosophy and psychology concerning emotion, disease, influence, state of being,",
                    "Affection or fondness is a disposition or rare state of mind or body that is often associated with a feeling or type of love. It has given rise to a number of branches of philosophy and psychology concerning emotion, disease, influence, state of being,Affection is popularly used to denote a feeling or type of love, amounting to more than goodwill or friendship. Writers on ethics generally use the word to refer to distinct states of feeling, both lasting and spasmodic. Some contrast it with passion as being free from the distinctively sensual element.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                    "Attachment",
                    "",
                    "Assets/23.png",
                    "Attachment theory describes the dynamics of long-term relationships between humans. Its most important tenet is that an infant needs to develop a relationship with at least one primary caregiver for social and emotional development to occur normally.",
                    "Attachment theory describes the dynamics of long-term relationships between humans. Its most important tenet is that an infant needs to develop a relationship with at least one primary caregiver for social and emotional development to occur normally. Attachment theory explains how much the parents' relationship with the child influences development. Attachment theory is an interdisciplinary study encompassing the fields of psychological, evolutionary, and ethological theory. Immediately after World War II, homeless and orphaned children presented many difficulties, and psychiatrist and psychoanalyst John Bowlby was asked by the UN to write a pamphlet on the issue which he entitled maternal deprivation. Attachment theory grew out of his subsequent work on the issues raised.s",
                    group2));
            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                    "Directions",
                    "",
                    "Assets/30.png",
                    "");
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                    "Cultural identity",
                    "",
                    "Assets/31.png",
                    "Cultural identity is the identity of a group or culture, or of an individual as far as one is influenced by one's belonging to a group or culture. Cultural identity is similar to and overlaps with, identity politics.",
                    "Various modern cultural studies and social theories have investigated cultural identity. In recent decades, a new form of identification has emerged which breaks down the understanding of the individual as a coherent whole subject into a collection of various cultural identifiers. These cultural identifiers may be the result of various conditions including: location, gender, race, history, nationality, language, sexuality, religious beliefs, ethnicity, aesthetics, and even food. The divisions between cultures can be very fine in some parts of the world, especially places such as Canada or the United States, where the population is ethnically diverse and social unity is based primarily on common social values and beliefs.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                    "Apathy",
                    "",
                    "Assets/32.png",
                    "Apathy (also called impassivity or perfunctoriness) is a state of indifference, or the suppression of emotions such as concern, excitement, motivation and passion.",
                    "Apathy (also called impassivity or perfunctoriness) is a state of indifference, or the suppression of emotions such as concern, excitement, motivation and passion. An apathetic individual has an absence of interest in or concern about emotional, social, spiritual, philosophical and/or physical life.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                    "Friendship",
                    "",
                    "Assets/33.png",
                    "Friendship is a relationship of mutual affection between two or more people. Friendship is a stronger form of interpersonal bond than an acquaintanceship. Friendship has been studied in academic fields such as sociology, social psychology, anthropology, and philosophy.",
                    "Friendship is a relationship of mutual affection between two or more people. Friendship is a stronger form of interpersonal bond than an acquaintanceship. Friendship has been studied in academic fields such as sociology, social psychology, anthropology, and philosophy. Various academic theories of friendship have been proposed, including social exchange theory, equity theory, relational dialectics, and attachment styles.\n\nAlthough there are many forms of friendship, some of which may vary from place to place, certain characteristics are present in many types of friendship. Such characteristics include affection, sympathy, empathy, honesty, altruism, mutual understanding and compassion, enjoyment of each other's company, trust, and the ability to be oneself, express one's feelings, and make mistakes without fear of judgment from the friend. While there is no practical limit on what types of people can form a friendship, friends tend to share common backgrounds, occupations, or interests, and have similar demographics.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-4",
                    "Essence",
                    "",
                    "Assets/34.png",
                    "In philosophy, essence is the attribute or set of attributes that make an entity or substance what it fundamentally is, and which it has by necessity, and without which it loses its identity.",
                    "In philosophy, essence is the attribute or set of attributes that make an entity or substance what it fundamentally is, and which it has by necessity, and without which it loses its identity. Essence is contrasted with accident: a property that the entity or substance has contingently, without which the substance can still retain its identity. The concept originates with Aristotle, who used the Greek expression to ti ên einai, literally 'the what it was to be', or sometimes the shorter phrase to ti esti, literally 'the what it is,' for the same idea. This phrase presented such difficulties for his Latin translators that they coined the word essentia (English essence) to represent the whole expression. For Aristotle and his scholastic followers the notion of essence is closely linked to that of definition (horismos).",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-5",
                    "Altruism",
                    "",
                    "Assets/35.png",
                    "Altruism or selflessness is the principle or practice of concern for the welfare of others. It is a traditional virtue in many cultures and a core aspect of various religious traditions, though the concept of others toward whom concern should be directed can vary among cultures and religions.",
                    "Altruism or selflessness is the principle or practice of concern for the welfare of others. It is a traditional virtue in many cultures and a core aspect of various religious traditions, though the concept of others toward whom concern should be directed can vary among cultures and religions. Altruism or selflessness is the opposite of selfishness.\n\nAltruism can be distinguished from feelings of duty and loyalty. Altruism is a motivation to provide something of value to a party who must be anyone but one's self, while duty focuses on a moral obligation towards a specific individual (e.g., a god, a king), or collective (e.g., a government). Pure altruism consists of sacrificing something for someone other than the self (e.g. sacrificing time, energy or possessions) with no expectation of any compensation or benefits, either direct, or indirect (e.g., receiving recognition for the act of giving).",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-6",
                    "Erotomania",
                    "",
                    "Assets/36.png",
                    "Erotomania is a type of delusion in which the affected person believes that another person, usually a stranger, high-status or famous person, is in love with him or her. The illness often occurs during psychosis, especially in patients with schizophrenia, delusional disorder or bipolar mania.",
                    "Erotomania is a type of delusion in which the affected person believes that another person, usually a stranger, high-status or famous person, is in love with him or her. The illness often occurs during psychosis, especially in patients with schizophrenia, delusional disorder or bipolar mania.[1] During an erotomanic episode, the patient believes that a secret admirer is declaring his or her affection to the patient, often by special glances, signals, telepathy, or messages through the media. Usually the patient then returns the perceived affection by means of letters, phone calls, gifts, and visits to the unwitting recipient. Even though these advances are unexpected and often unwanted, any denial of affection by the object of this delusional love is dismissed by the patient as a ploy to conceal the forbidden love from the rest of the world.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-7",
                    "Mania",
                    "",
                    "Assets/37.png",
                    "Mania is a state of abnormally elevated or irritable mood, arousal, and/or energy levels.[1] In a sense, it is the opposite of depression. Mania is a criterion for certain psychiatric diagnoses.",
                    "In addition to mood disorders, persons may exhibit manic behaviour because of drug intoxication (notably stimulants, such as cocaine and methamphetamine), medication side effects (notably steroids and SSRIs), and malignancy. But mania is most often associated with bipolar disorder, where episodes of mania may alternate with episodes of major depression. Gelder, Mayou, and Geddes (2005) suggest that it is vital that mania be predicted in the early stages because otherwise the patient becomes reluctant to comply to the treatment. The criteria for bipolar disorder do not include depressive episodes, and the presence of mania in the absence of depressive episodes is sufficient for a diagnosis. Regardless, those who never experience depression also experience cyclical changes in mood. These cycles are often affected by changes in sleep cycle (too much or too little), diurnal rhythms, and environmental stressors.",
                    group3));
            this.AllGroups.Add(group3);
        }
    }
}
